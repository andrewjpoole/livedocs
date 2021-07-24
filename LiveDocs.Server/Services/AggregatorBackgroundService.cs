using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using AJP.SimpleScheduler.Intervals;
using AJP.SimpleScheduler.ScheduledTasks;
using AJP.SimpleScheduler.ScheduledTaskStorage;
using AJP.SimpleScheduler.TaskExecution;
using LiveDocs.Server.config;
using LiveDocs.Server.Models;
using LiveDocs.Server.Replacements;
using LiveDocs.Server.Replacers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using File = System.IO.File;

namespace LiveDocs.Server.Services
{
    /// <summary>
    /// Service which fetches markdown + configJson for a resource and serves markdown 
    /// </summary>
    public class AggregatorBackgroundService : IAggregatorBackgroundService
    {
        private readonly IOptions<StronglyTypedConfig.LiveDocs> _liveDocsOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly IScheduledTaskRepository _scheduledTaskRepository;
        private readonly IScheduledTaskBuilderFactory _scheduledTaskBuilderFactory;
        private readonly ILogger<AggregatorBackgroundService> _logger;
        private const int WaitTimeInMs = 5000;
        private const string ReplacementPrefix = "<<";
        private const string ReplacementSuffix = ">>";
        private Dictionary<string, ResourceDocumentation> _resourceDocumentations = new();
        
        public AggregatorBackgroundService(
            IOptions<StronglyTypedConfig.LiveDocs> liveDocsOptions, 
            IServiceProvider serviceProvider, 
            IScheduledTaskRepository scheduledTaskRepository, 
            IDueTaskJobQueue dueTaskJobQueue, 
            IScheduledTaskBuilderFactory scheduledTaskBuilderFactory,
            ILogger<AggregatorBackgroundService> logger)
        {
            _liveDocsOptions = liveDocsOptions;
            _serviceProvider = serviceProvider;
            _scheduledTaskRepository = scheduledTaskRepository;
            _scheduledTaskBuilderFactory = scheduledTaskBuilderFactory;
            _logger = logger;

            dueTaskJobQueue.RegisterHandlerWhen(RunDueScheduledTask, task => task.JobDataTypeName == "Object");
            //dueTaskJobQueue.RegisterHandlerWhen(HandleReloadResourceDocumentations, task => task.JobDataTypeName == nameof(String));

            LoadResourceDocumentations();

            //_scheduledTaskRepository.AddScheduledTask(_scheduledTaskBuilderFactory.CreateBuilder().EveryStartingAt(Lapse.Minutes(5), DateTime.UtcNow).WithJobData("ReloadResourceDocumentations"));
        }

        private void HandleReloadResourceDocumentations(IScheduledTask obj)
        {
            _logger.LogInformation("Clearing scheduled tasks and Resource Documentation files...");
            // clear scheduled tasks
            foreach (var scheduledTask in _scheduledTaskRepository.AllTasks())
            {
                _scheduledTaskRepository.RemoveScheduledTask(scheduledTask.Id);
            }

            _scheduledTaskRepository.AddScheduledTask(_scheduledTaskBuilderFactory.CreateBuilder().EveryStartingAt(Lapse.Seconds(30), DateTime.UtcNow).WithJobData("ReloadResourceDocumentations"));

            _resourceDocumentations = new Dictionary<string, ResourceDocumentation>();

            
        }

        private void LoadResourceDocumentations()
        {
            _logger.LogInformation("Loading Resource Documentation files...");
            var resourceDocumentationFilesJson = GetFileContents(_liveDocsOptions.Value.ResourceDocumentationFileListing);
            var resourceDocFiles = JsonSerializer.Deserialize<ResourceDocumentationFileListing>(resourceDocumentationFilesJson);

            // ToDo check if content has changed before updating?

            foreach (var file in resourceDocFiles.Files)
            {
                var markdown = GetFileContents(file.MdPath);
                var json = GetFileContents(file.JsonPath);
                var newResourceDocumentation = new ResourceDocumentation
                {
                    Name = file.Name,
                    RawMarkdown = markdown,
                    Replacements = JsonSerializer.Deserialize<ReplacementConfig>(json,
                            new JsonSerializerOptions
                            {
                                AllowTrailingCommas = true,
                                ReadCommentHandling = JsonCommentHandling.Skip
                            })
                        .Replacements
                };
                _resourceDocumentations.Add(file.Name, newResourceDocumentation);

                // Register replacements as scheduled tasks
                foreach (var replacement in newResourceDocumentation.Replacements)
                {
                    replacement.ParentResourceDocumentationName = newResourceDocumentation.Name;
                    _scheduledTaskRepository.AddScheduledTask(_scheduledTaskBuilderFactory.CreateBuilder().FromString(replacement.Schedule, replacement));
                }
            }
        }

        private string GetFileContents(string filePath)
        {
            if (filePath.StartsWith("http"))
            {
                return new WebClient().DownloadString(filePath);
            }

            return File.ReadAllText(filePath);
        }

        private void RunDueScheduledTask(IScheduledTask scheduledTask)
        {
            var replacement = JsonSerializer.Deserialize<Replacement>(scheduledTask.JobData);
            if (replacement is null)
                throw new ApplicationException(
                    $"Unable to run scheduled task {scheduledTask.Id}, jobData does not contain a serialised instance of a Replacement");

            _logger.LogInformation($"Running scheduled task {scheduledTask.Id} for {replacement.Instruction}.{replacement.Match}");

            try
            {
                var replacer = (IReplacer)_serviceProvider.GetServiceByRegisteredTypeName(replacement.Instruction);
                var renderedValue = replacer.Render(replacement.Match).GetAwaiter().GetResult();

                // store the result in the appropriate ResourceDocumentation's replacement
                _resourceDocumentations[replacement.ParentResourceDocumentationName].Replacements
                    .FirstOrDefault(r => r.Match == replacement.Match).LatestReplacedData = renderedValue; // ToDO find a nicer way
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error thrown while compiling markdown {e.Message}");
                _resourceDocumentations[replacement.ParentResourceDocumentationName].Replacements
                    .FirstOrDefault(r => r.Match == replacement.Match).LatestReplacedData = $"Failed to replace {replacement.Match}!";
            }
        }
        
        // Called by the RequestHandler
        public string GetLatestMarkdown(string resourceName)
        {
            var resourceDocumentation = _resourceDocumentations[resourceName];
            var renderedMarkdown = resourceDocumentation.RawMarkdown;

            foreach (var replacement in resourceDocumentation.Replacements)
            {
                // TODO use spans instead
                renderedMarkdown = renderedMarkdown.Replace($"{ReplacementPrefix}{replacement.Match}{ReplacementSuffix}", replacement.LatestReplacedData);
            }

            return renderedMarkdown;
        }

        public void ReloadResourceDocumentationFiles()
        {
            _logger.LogInformation("ReloadResourceDocumentationFiles requested via api call.");
            LoadResourceDocumentations();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AJP.SimpleScheduler.ScheduledTasks;
using AJP.SimpleScheduler.ScheduledTaskStorage;
using AJP.SimpleScheduler.TaskExecution;
using LiveDocs.Server.config;
using LiveDocs.Server.Replacements;
using LiveDocs.Server.Replacers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveDocs.Server.Services
{
    /// <summary>
    /// Service which fetches markdown + configJson for a resource and serves markdown 
    /// </summary>
    public class AggregatorBackgroundService : IAggregatorBackgroundService
    {
        private readonly IOptions<StronglyTypedConfig.LiveDocs> _liveDocsOptions;
        private readonly IServiceProvider _serviceProvider;
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
            _logger = logger;

            foreach (var file in _liveDocsOptions.Value.Files)
            {
                var newResourceDocumentation = new ResourceDocumentation
                {
                    Name = file.Name,
                    RawMarkdown = File.ReadAllText(file.MdPath), // TODO when should this be checked for updates?
                    Replacements = JsonSerializer.Deserialize<ReplacementConfig>(File.ReadAllText(file.JsonPath),
                            new JsonSerializerOptions
                            {
                                AllowTrailingCommas = true, 
                                ReadCommentHandling = JsonCommentHandling.Skip
                            })
                        .Replacements // TODO when should the markdown and json be checked for updates ?
                };
                _resourceDocumentations.Add(file.Name, newResourceDocumentation);

                // Register replacements as scheduled tasks
                foreach (var replacement in newResourceDocumentation.Replacements)
                {
                    replacement.ParentResourceDocumentationName = newResourceDocumentation.Name;
                    scheduledTaskRepository.AddScheduledTask(scheduledTaskBuilderFactory.CreateBuilder().FromString(replacement.Schedule, replacement));
                }
            }

            dueTaskJobQueue.RegisterHandlerForAllTasks(RunDueScheduledTask);
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
            var testDataService = new TransactionTestData();
            testDataService.InsertSomeRandomTransactionRows().GetAwaiter().GetResult();

            //return _resourceDocumentations[resourceName].RenderedMarkdown;
            var resourceDocumentation = _resourceDocumentations[resourceName];
            var renderedMarkdown = resourceDocumentation.RawMarkdown;

            foreach (var replacement in resourceDocumentation.Replacements)
            {
                // TODO use spans instead
                //var replacementValue = await FetchReplacementValue(replacement);
                renderedMarkdown = renderedMarkdown.Replace($"{ReplacementPrefix}{replacement.Match}{ReplacementSuffix}", replacement.LatestReplacedData);
            }

            return renderedMarkdown;
        }
    }
}
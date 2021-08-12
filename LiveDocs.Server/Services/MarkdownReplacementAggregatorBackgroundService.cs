using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LiveDocs.Server.config;
using LiveDocs.Server.Hubs;
using LiveDocs.Server.Models;
using LiveDocs.Server.Replacements;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace LiveDocs.Server.Services
{
    /// <summary>
    /// Service which fetches resource documentation files, registers replacements with the cache and serves the replaced markdown on a timer
    /// </summary>
    public class MarkdownReplacementAggregatorBackgroundService : IHostedService, IMarkdownReplacementAggregatorBackgroundService, IDisposable
    {
        private Timer _timer;
        private readonly IOptions<StronglyTypedConfig.LiveDocs> _liveDocsOptions;
        private readonly ILogger<MarkdownReplacementAggregatorBackgroundService> _logger;
        private readonly IFileContentDownloader _fileContentDownloader;
        private readonly IReplacementCache _replacementCache;
        private readonly IHubContext<LatestMarkdownHub> _latestMarkdownHub;
        private StringBuilder _markdownBuilder;
        private const string ReplacementPrefix = "<<";
        private const string ReplacementSuffix = ">>";
        private Dictionary<string, ResourceDocumentation> _resourceDocumentations = new();
        private string PreviousResourceDocumentationFilesJsonHash { get; set; }
        private JsonSerializerOptions _jsonOptions = new()
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };


        public MarkdownReplacementAggregatorBackgroundService(
            IOptions<StronglyTypedConfig.LiveDocs> liveDocsOptions, 
            ILogger<MarkdownReplacementAggregatorBackgroundService> logger, 
            IFileContentDownloader fileContentDownloader,
            IReplacementCache replacementCache, IHubContext<LatestMarkdownHub> latestMarkdownHub)
        {
            _liveDocsOptions = liveDocsOptions;
            _logger = logger;
            _fileContentDownloader = fileContentDownloader;
            _replacementCache = replacementCache;
            _latestMarkdownHub = latestMarkdownHub;

            _ = LoadResourceDocumentations();
        }
        
        private async Task LoadResourceDocumentations()
        {
            _logger.LogInformation("Loading Resource Documentation files...");
            var resourceDocumentationFilesJson = await GetFileContents(_liveDocsOptions.Value.ResourceDocumentationFileListing);


            var resourceDocumentationFilesJsonHash = HashString(resourceDocumentationFilesJson);
            if (resourceDocumentationFilesJsonHash == PreviousResourceDocumentationFilesJsonHash)
               return;

            PreviousResourceDocumentationFilesJsonHash = resourceDocumentationFilesJsonHash;

            // Clear out old state...
            _resourceDocumentations = new();
            _replacementCache.ClearCache();

            var resourceDocFiles = JsonSerializer.Deserialize<ResourceDocumentationFileListing>(resourceDocumentationFilesJson);
            
            foreach (var file in resourceDocFiles.Files)
            {
                var markdown = await GetFileContents(file.MdPath);
                var json = await GetFileContents(file.JsonPath);
                var newResourceDocumentation = new ResourceDocumentation
                {
                    Name = file.Name,
                    RawMarkdown = markdown,
                    Replacements = JsonSerializer.Deserialize<ReplacementConfig>(json, _jsonOptions).Replacements
                };
                _resourceDocumentations.Add(file.Name, newResourceDocumentation);
                
                // register replacements in cache
                foreach (var replacement in newResourceDocumentation.Replacements)
                {
                    _replacementCache.RegisterReplacement(replacement.Match, replacement.Instruction, replacement.TimeToLive, true);
                }
            }
        }
        
        private string HashString(string toHash)
        {
            var bytesToHash = Encoding.Default.GetBytes(toHash);
            var hashedBytes = SHA256.HashData(bytesToHash);
            var hashAsString = Encoding.Default.GetString(hashedBytes);
            return hashAsString;
        }

        private async Task<string> GetFileContents(string filePath)
        {
            if (filePath.StartsWith("http"))
            {
                return await _fileContentDownloader.Fetch(filePath);
            }

            return await File.ReadAllTextAsync(filePath);
        }
        
        // Called by the RequestHandler
        public async Task ReloadResourceDocumentationFiles()
        {
            _logger.LogInformation("ReloadResourceDocumentationFiles requested via api call.");
            await LoadResourceDocumentations();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MarkdownReplacementAggregatorBackgroundService running.");
            
            _timer = new Timer(10_000);
            _timer.Elapsed += async (sender, e) => await DoWork();
            _timer.Start();

            return Task.CompletedTask;
        }

        private async Task DoWork()
        {
            var sw = new Stopwatch();
            sw.Start();

            foreach (var resource in _resourceDocumentations.Values)
            {
                // ToDO figure out if there are any connected clients in this resource group and skip if not?

                _markdownBuilder = new StringBuilder(resource.RawMarkdown);

                // kick of all replacement tasks in parallel and then wait for them all to complete
                var tasks = resource.Replacements.Select(
                    async r => await _replacementCache.FetchCurrentReplacementValue(r.Match, r.Instruction, false));

                var results = await Task.WhenAll(tasks);

                foreach (var replacedValue in results.ToList())
                {
                    _markdownBuilder.Replace($"{ReplacementPrefix}{replacedValue.Name}{ReplacementSuffix}", replacedValue.Data);
                }

                _logger.LogInformation($"sending markdown for {resource.Name}");
                await _latestMarkdownHub.Clients.Group(resource.Name).SendAsync("SendLatestMarkdownToInterestedClients", _markdownBuilder.ToString());
            }
            sw.Stop();
            _logger.LogInformation($"timer DoWork completed after {sw.ElapsedMilliseconds}ms");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MarkdownReplacementAggregatorBackgroundService stopping.");

            _timer?.Stop();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
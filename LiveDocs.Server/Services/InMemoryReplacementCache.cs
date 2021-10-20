using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Kusto.Data.Common;
using LiveDocs.Server.Replacements;
using LiveDocs.Server.Replacers;
using LiveDocs.Server.RequestHandlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveDocs.Server.Services
{
    public class InMemoryReplacementCache : BackgroundService, IReplacementCache, IInMemoryReplacementCacheBackgroundTaskQueueStats
    {
        private readonly ILogger<InMemoryReplacementCache> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private Dictionary<string, Replacement> _replacements = new();

        public InMemoryReplacementCache(ILogger<InMemoryReplacementCache> logger, IServiceProvider serviceProvider, IBackgroundTaskQueue backgroundTaskQueue)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public void ClearCache()
        {
            _replacements = new();
        }

        public void RegisterReplacement(string name, string instruction, string timeToLive, bool replaceIfKeyExists)
        {
            var key = $"{instruction}:{name}";
            if (_replacements.ContainsKey(key))
            {
                if (replaceIfKeyExists)
                {
                    _replacements.Remove(key);
                }
                else
                {
                    _logger.LogWarning($"A replacement with key {key} has already been registered in the InMemoryReplacementCache");
                    return;
                }
            }

            _replacements.Add(key, new Replacement
            {
                Instruction = instruction,
                Match = name,
                TimeToLive = timeToLive,
                Expired = CalculateExpiry(timeToLive)
            });
        }

        public async Task<(string Name, string Data)> FetchCurrentReplacementValue(string name, string instruction, bool waitForNewValueIfExpired)
        {
            var key = $"{instruction}:{name}";

            // requested replacement must exist
            if (!_replacements.ContainsKey(key)) return (name, $"replacement with key {key} not found");

            // if requested replacement is current, return it
            if (!_replacements[key].HasExpired()) return (name, _replacements[key].LatestReplacedData);

            if (waitForNewValueIfExpired)
            {
                //_logger.LogDebug($"waiting for expired replacement to be re-fetched {name}");
                await RunReplacement(_replacements[key]);
                return (name, _replacements[key].LatestReplacedData);
            }

            // check if we have already scheduled the replacement
            if(_replacements[key].IsScheduled)
                return (name, _replacements[key].LatestReplacedData);

            // otherwise schedule a new replacement
            await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(
                async token => await RunReplacement(_replacements[key]));

            _replacements[key].IsScheduled = true;

            // finally return the expired replacement value as its better than nothing and should be replaced by next time
            return (name, _replacements[key].LatestReplacedData);
        }

        private async Task RunReplacement(Replacement replacement)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                //_logger.LogInformation($"Running {replacement.Match} using {replacement.Instruction} with TTL {replacement.TimeToLive}");
                var replacer = (IReplacer)_serviceProvider.GetServiceByRegisteredTypeName(replacement.Instruction);
                var renderedValue = await replacer.Render(replacement.Match);
                replacement.LatestReplacedData = renderedValue;
                replacement.IsScheduled = false;

                stopwatch.Stop();
                _logger.LogInformation($"{replacement.Match}:{replacement.Instruction} completed after {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error thrown while compiling markdown:\n{e.Message}");
                replacement.LatestReplacedData = $"Failed to replace {replacement.Match}!";
            }
        }

        private DateTime CalculateExpiry(string timeToLive)
        {
            var now = DateTime.UtcNow;
            var timeToLiveTimeSpan = CslTimeSpanLiteral.Parse(timeToLive);
            if (timeToLiveTimeSpan.HasValue)
                return now + timeToLiveTimeSpan.Value;
            
            _logger.LogWarning($"Unable to parse kusto timespan from {timeToLive}");
            return DateTime.MinValue;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"InMemoryReplacementCache Hosted Service is running.");

            await BackgroundProcessing(cancellationToken);
        }
        
        private async Task BackgroundProcessing(CancellationToken cancellationToken)
        {
            // consume the queue of expired replacements and re-fetch data
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug($"{_backgroundTaskQueue.ItemCount} items to process");
                var workItem = await _backgroundTaskQueue.DequeueAsync(cancellationToken);

                try
                {
                    await workItem(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing {WorkItem}.", nameof(workItem));
                }
            }
        }

        public async Task<object> GetStats()
        {
            return new
            {
                OutstandingTaskCount = _backgroundTaskQueue.ItemCount
            };
        }
    }
}
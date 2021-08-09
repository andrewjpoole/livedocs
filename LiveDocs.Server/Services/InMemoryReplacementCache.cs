using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Kusto.Data.Common;
using LiveDocs.Server.Replacements;
using LiveDocs.Server.Replacers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveDocs.Server.Services
{
    public class InMemoryReplacementCache : BackgroundService, IReplacementCache
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

        public async Task<string> FetchCurrentReplacementValue(string name, string instruction, bool waitForNewValueIfExpired)
        {
            var key = $"{instruction}:{name}";

            if (_replacements.ContainsKey(key))
            {
                if (waitForNewValueIfExpired)
                {
                    _logger.LogDebug($"waiting for expired replacement to be re-fetched {name}");
                    await RunReplacement(_replacements[key]);
                }
                else
                {
                    _logger.LogDebug($"replacement named {name} has expired and will be re-fetched in the background");
                    // ToDo add job to queue to trigger RunReplacement?
                    await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(
                        async token => await RunReplacement(_replacements[key]));
                }

                return _replacements[key].LatestReplacedData;
            }

            return $"replacement with key {key} not found";
        }

        public void ClearCache()
        {
            _replacements = new();
        }

        private async Task RunReplacement(Replacement replacement)
        {
            try
            {
                _logger.LogInformation($"Running {replacement.Match} using {replacement.Instruction} with TTL {replacement.TimeToLive}");
                var replacer = (IReplacer)_serviceProvider.GetServiceByRegisteredTypeName(replacement.Instruction);
                var renderedValue = await replacer.Render(replacement.Match);
                replacement.LatestReplacedData = renderedValue;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error thrown while compiling markdown {e.Message}");
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
            _logger.LogInformation($"Queued Hosted Service is running.");

            await BackgroundProcessing(cancellationToken);
        }
        
        private async Task BackgroundProcessing(CancellationToken cancellationToken)
        {
            // consume the queue of expired replacements and re-fetch data
            while (!cancellationToken.IsCancellationRequested)
            {
                var workItem = await _backgroundTaskQueue.DequeueAsync(cancellationToken);

                try
                {
                    await workItem(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error occurred executing {WorkItem}.", nameof(workItem));
                }
            }
        }
    }

    public interface IBackgroundTaskQueue
    {
        ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);

        ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
            CancellationToken cancellationToken);
    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

        public BackgroundTaskQueue(int capacity)
        {
            // Capacity should be set based on the expected application load and
            // number of concurrent threads accessing the queue.            
            // BoundedChannelFullMode.Wait will cause calls to WriteAsync() to return a task,
            // which completes only when space became available. This leads to backpressure,
            // in case too many publishers/calls start accumulating.
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
        }

        public async ValueTask QueueBackgroundWorkItemAsync(
            Func<CancellationToken, ValueTask> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            await _queue.Writer.WriteAsync(workItem);
        }

        public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
            CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);

            return workItem;
        }
    }
}
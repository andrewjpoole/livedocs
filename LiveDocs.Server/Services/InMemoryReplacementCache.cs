using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kusto.Data.Common;
using LiveDocs.Server.Replacements;
using LiveDocs.Server.Replacers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveDocs.Server.Services
{
    public class InMemoryReplacementCache : IReplacementCache
    {
        private readonly ILogger<InMemoryReplacementCache> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, Replacement> _replacements = new();

        public InMemoryReplacementCache(ILogger<InMemoryReplacementCache> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
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

        //protected override Task ExecuteAsync(CancellationToken cancellationToken)
        //{
        //    // consume the queue of expired replacements and re-fetch data
        //    while (!cancellationToken.IsCancellationRequested)
        //    {

        //    }
        //}
    }
}
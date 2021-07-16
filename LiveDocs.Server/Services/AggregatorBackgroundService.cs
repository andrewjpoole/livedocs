using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LiveDocs.Server.config;
using LiveDocs.Server.Replacements;
using LiveDocs.Server.Replacers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LiveDocs.Server.Services
{
    /// <summary>
    /// Service which fetches markdown + configJson for a resource and serves markdown 
    /// </summary>
    public class AggregatorBackgroundService : BackgroundService, IAggregatorBackgroundService
    {
        private readonly IOptions<StronglyTypedConfig.LiveDocs> _liveDocsOptions;
        private const int WaitTimeInMs = 5000;
        private const string ReplacementPrefix = "{{";
        private const string ReplacementSuffix = "}}";
        private Dictionary<string, ResourceDocumentation> _resourceDocumentations = new();
        private Dictionary<string, IReplacer> _replacers;
        
        public AggregatorBackgroundService(IOptions<StronglyTypedConfig.AzureAd> azureAdOptions, IOptions<StronglyTypedConfig.LiveDocs> liveDocsOptions, IConfiguration configuration)
        {
            _liveDocsOptions = liveDocsOptions;
            _replacers = new Dictionary<string, IReplacer> // TODO get these from DI -> probably use the interface name as the instruction?
            {
                {"SvcBusMessageInfo", new SvcBusMessageInfo(new AzureRMApiClient(new AzureIAMTokenFetcher(azureAdOptions), liveDocsOptions), azureAdOptions, liveDocsOptions)},
                {"SqlStoredProcInfo", new SqlStoredProcInfo(configuration)}
            };

            foreach (var file in _liveDocsOptions.Value.Files)
            {
                _resourceDocumentations.Add("test", new ResourceDocumentation
                {
                    Name = file.Name,
                    RawMarkdown = File.ReadAllText(file.MdPath),
                    Replacements = JsonSerializer.Deserialize<ReplacementConfig>(File.ReadAllText(file.JsonPath)).Replacements
                });
            }
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var testDataService = new TransactionTestData();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(WaitTimeInMs);

                await testDataService.InsertSomeRandomTransactionRows();

                // TODO for each due task? - json should also contain refresh period?
                // TODO consider using SimpleScheduler?
                foreach (var resourceDocumentation in _resourceDocumentations.Values)
                {
                    await ReplaceTokens(resourceDocumentation);
                }
            }
        }

        public string GetLatestMarkdown(string resourceName)
        {
            return _resourceDocumentations[resourceName].RenderedMarkdown;
        }

        private async Task ReplaceTokens(ResourceDocumentation resourceDocumentation)
        {
            try
            {
                var rawMarkdown = resourceDocumentation.RawMarkdown;

                foreach (var replacement in resourceDocumentation.Replacements)
                {
                    Console.WriteLine(replacement.Match);

                    // TODO only run replacer if value is found in markdown?
                    //if(!rawMarkdown.Contains($"{ReplacementPrefix}{replacement.Match}{ReplacementSuffix}"))
                    //    continue;

                    // TODO use spans instead
                    var replacementValue = await FetchReplacementValue(replacement);
                    rawMarkdown = rawMarkdown.Replace($"{ReplacementPrefix}{replacement.Match}{ReplacementSuffix}", replacementValue);
                }

                resourceDocumentation.RenderedMarkdown = rawMarkdown;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task<string> FetchReplacementValue(Replacement replacement)
        {
            try
            {
                var replacer = _replacers[replacement.Instruction];
                var renderedValue = await replacer.Render(replacement.Match);
                return renderedValue;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return $"Failed to replace {replacement.Match}!";
            }
        }
    }
}
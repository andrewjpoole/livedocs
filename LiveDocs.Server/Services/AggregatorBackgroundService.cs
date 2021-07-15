using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LiveDocs.Server.Replacements;
using LiveDocs.Server.Replacers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace LiveDocs.Server.Services
{
    /// <summary>
    /// Service which fetches markdown + configJson for a resource and serves markdown 
    /// </summary>
    public class AggregatorBackgroundService : BackgroundService, IAggregatorBackgroundService
    {
        private const int WaitTimeInMs = 5000;
        private const string ReplacementPrefix = "{{";
        private const string ReplacementSuffix = "}}";
        private Dictionary<string, ResourceDocumentation> _resourceDocumentations = new();
        private Dictionary<string, IReplacer> _replacers;
        
        public AggregatorBackgroundService(IConfiguration configuration)
        {
            _replacers = new Dictionary<string, IReplacer> // TODO get these from DI
            {
                {"SvcBusMessageInfo", new SvcBusMessageInfo(new AzureRMApiClient(new AzureIAMTokenFetcher(configuration), configuration), configuration)},
                {"SqlStoredProcInfo", new SqlStoredProcInfo(configuration)}
            };
            
            // hardcode for now...
            _resourceDocumentations.Add("test", new ResourceDocumentation
            {
                Name = "test",
                RawMarkdown = File.ReadAllText("C:\\git\\livedocs\\ResourceDocumentations\\article2.md"),
                Replacements = JsonSerializer.Deserialize<ReplacementConfig>(File.ReadAllText("C:\\git\\livedocs\\ResourceDocumentations\\article2.json")).Replacements
            });
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var testDataService = new TransactionTestData();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(WaitTimeInMs);

                await testDataService.InsertSomeRandomTransactionRows();

                // for each due task? - json should also contain refresh period?
                // consider using SimpleScheduler?
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
            var rawMarkdown = resourceDocumentation.RawMarkdown;
            
            foreach (var replacement in resourceDocumentation.Replacements)
            {
                Console.WriteLine(replacement.Match);

                //if(!rawMarkdown.Contains($"{ReplacementPrefix}{replacement.Match}{ReplacementSuffix}"))
                //    continue;

                var replacer = _replacers[replacement.Instruction];
                var renderedValue = await replacer.Render(replacement.Match);
                rawMarkdown = rawMarkdown.Replace($"{ReplacementPrefix}{replacement.Match}{ReplacementSuffix}", renderedValue);
                // TODO use spans
            }

            resourceDocumentation.RenderedMarkdown = rawMarkdown;
        }
    }
}
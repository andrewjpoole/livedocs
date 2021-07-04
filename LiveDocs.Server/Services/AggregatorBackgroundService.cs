using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LiveDocs.Server.Replacements;
using LiveDocs.Server.Replacers;
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

        public AggregatorBackgroundService()
        {
            _replacers = new Dictionary<string, IReplacer>
            {
                {"SvcBusMessageInfo", new SvcBusMessageInfo()},
                {"SqlStoredProcInfo", new SqlStoredProcInfo()}
            };
            
            // hardcode for now...
            _resourceDocumentations.Add("test", new ResourceDocumentation
            {
                Name = "test",
                RawMarkdown = File.ReadAllText("C:\\dev\\livedocs\\ResourceDocumentations\\article2.md"),
                Replacements = JsonSerializer.Deserialize<ReplacementConfig>(File.ReadAllText("C:\\dev\\livedocs\\ResourceDocumentations\\article2.json")).Replacements
            });
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(WaitTimeInMs);

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
                //Console.WriteLine(rawMarkdown.Contains($"{ReplacementPrefix}{replacement.Match}{ReplacementSuffix}"));

                //if(!rawMarkdown.Contains($"{ReplacementPrefix}{replacement.Match}{ReplacementSuffix}"))
                //    continue;

                var replacer = _replacers[replacement.Instruction];
                var renderedValue = replacer.Render(replacement.Match);
                //Console.WriteLine(renderedValue);
                rawMarkdown = rawMarkdown.Replace($"{ReplacementPrefix}{replacement.Match}{ReplacementSuffix}", renderedValue);
                // TODO use spans
            }

            resourceDocumentation.RenderedMarkdown = rawMarkdown;
        }
    }
}
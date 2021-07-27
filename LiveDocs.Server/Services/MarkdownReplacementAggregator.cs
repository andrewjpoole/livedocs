using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LiveDocs.Server.config;
using LiveDocs.Server.Models;
using LiveDocs.Server.Replacements;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveDocs.Server.Services
{
    /// <summary>
    /// Service which fetches resource documentation files, registers replacements with the cache and serves the replaced markdown 
    /// </summary>
    public class MarkdownReplacementAggregator : IMarkdownReplacementAggregator
    {
        private readonly IOptions<StronglyTypedConfig.LiveDocs> _liveDocsOptions;
        private readonly ILogger<MarkdownReplacementAggregator> _logger;
        private readonly IReplacementCache _replacementCache;
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


        public MarkdownReplacementAggregator(
            IOptions<StronglyTypedConfig.LiveDocs> liveDocsOptions, 
            ILogger<MarkdownReplacementAggregator> logger, 
            IReplacementCache replacementCache)
        {
            _liveDocsOptions = liveDocsOptions;
            _logger = logger;
            _replacementCache = replacementCache;
            
            LoadResourceDocumentations();
        }
        
        private void LoadResourceDocumentations()
        {
            _logger.LogInformation("Loading Resource Documentation files...");
            var resourceDocumentationFilesJson = GetFileContents(_liveDocsOptions.Value.ResourceDocumentationFileListing);


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
                var markdown = GetFileContents(file.MdPath);
                var json = GetFileContents(file.JsonPath);
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

                // request the markdown to initialise the replacements...
                _ = GetLatestMarkdown(file.Name);
            }
        }
        
        private string HashString(string toHash)
        {
            var bytesToHash = Encoding.Default.GetBytes(toHash);
            var hashedBytes = SHA256.HashData(bytesToHash);
            var hashAsString = Encoding.Default.GetString(hashedBytes);
            return hashAsString;
        }

        private string GetFileContents(string filePath)
        {
            if (filePath.StartsWith("http"))
            {
                return new WebClient().DownloadString(filePath);
            }

            return File.ReadAllText(filePath);
        }

        // Called by the RequestHandler
        public async Task<string> GetLatestMarkdown(string resourceName)
        {
            _logger.LogDebug("GetLatestMarkdown requested via api call.");

            _markdownBuilder = new StringBuilder(_resourceDocumentations[resourceName].RawMarkdown);
            
            // kick of all replacement tasks in parallel and then wait for them all to complete
            var tasks = _resourceDocumentations[resourceName].Replacements.Select(
                async r => await _replacementCache.FetchCurrentReplacementValue(r.Match, r.Instruction, true));

            var results = await Task.WhenAll(tasks);

            foreach (var replacedValue in results.ToList())
            {
                _markdownBuilder.Replace($"{ReplacementPrefix}{replacedValue.Name}{ReplacementSuffix}", replacedValue.Data);
            }

            return _markdownBuilder.ToString();
        }

        // Called by the RequestHandler
        public void ReloadResourceDocumentationFiles()
        {
            _logger.LogInformation("ReloadResourceDocumentationFiles requested via api call.");
            LoadResourceDocumentations();
        }
    }
}
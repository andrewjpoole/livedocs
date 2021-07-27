using System;
using System.Collections.Generic;
using System.IO;
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
    /// Service which fetches markdown + configJson for a resource and serves markdown 
    /// </summary>
    public class AggregatorBackgroundService : IAggregatorBackgroundService
    {
        private readonly IOptions<StronglyTypedConfig.LiveDocs> _liveDocsOptions;
        private readonly ILogger<AggregatorBackgroundService> _logger;
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


        public AggregatorBackgroundService(
            IOptions<StronglyTypedConfig.LiveDocs> liveDocsOptions, 
            ILogger<AggregatorBackgroundService> logger, 
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

            foreach (var replacement in _resourceDocumentations[resourceName].Replacements)
            {
                _markdownBuilder.Replace($"{ReplacementPrefix}{replacement.Match}{ReplacementSuffix}", 
                    await _replacementCache.FetchCurrentReplacementValue(replacement.Match, replacement.Instruction, false));
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
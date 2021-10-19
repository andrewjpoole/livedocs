using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LiveDocs.Server.config;
using LiveDocs.Server.Models;
using LiveDocs.Server.Services;
using MediatR;
using Microsoft.Extensions.Options;

namespace LiveDocs.Server.RequestHandlers
{
    public class GetResourceDocumentationsRequest : IRequest<GetResourceDocumentationsResponse>
    {
    }

    public class GetResourceDocumentationsFile
    {
        public GetResourceDocumentationsFile(string name, string mdPath, string jsonPath, string domain, string subDomain)
        {
            Name = name;
            MdPath = mdPath;
            JsonPath = jsonPath;
            Domain = domain;
            SubDomain = subDomain;
        }

        public string Name { get; }
        public string MdPath { get; }
        public string JsonPath { get; }
        public string Domain { get; }
        public string SubDomain { get; }
    }

    public class GetResourceDocumentationsResponse
    {
        public GetResourceDocumentationsResponse(IEnumerable<GetResourceDocumentationsFile> files)
        {
            Files = files;
        }

        public IEnumerable<GetResourceDocumentationsFile> Files { get; init; }
    }

    public class GetResourceDocumentationsRequestHandler : IRequestHandler<GetResourceDocumentationsRequest, GetResourceDocumentationsResponse>
    {
        private readonly IOptions<StronglyTypedConfig.LiveDocs> _liveDocsOptions;
        private readonly IFileContentDownloader _fileContentDownloader;

        public GetResourceDocumentationsRequestHandler(IOptions<StronglyTypedConfig.LiveDocs> liveDocsOptions, IFileContentDownloader fileContentDownloader)
        {
            _liveDocsOptions = liveDocsOptions;
            _fileContentDownloader = fileContentDownloader;
        }

        public async Task<GetResourceDocumentationsResponse> Handle(GetResourceDocumentationsRequest request, CancellationToken cancellationToken)
        {
            var resourceDocumentationFilesJson = await _fileContentDownloader.Fetch(_liveDocsOptions.Value.ResourceDocumentationFileListing);
            var resourceDocFiles = JsonSerializer.Deserialize<ResourceDocumentationFileListing>(resourceDocumentationFilesJson);

            if (resourceDocFiles is null)
                throw new Exception("Could not fetch and deserialize Resource Documentation files");

            var response = new GetResourceDocumentationsResponse(
                resourceDocFiles.Files.Select(t => new GetResourceDocumentationsFile(
                    t.Name,
                    t.MdPath,
                    t.JsonPath,
                    t.Domain,
                    t.SubDomain)));
            return response;
        }
    }
}

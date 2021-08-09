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
        public string Name { get; set; }
        public string MdPath { get; set; }
        public string JsonPath { get; set; }
        public string Domain { get; set; }
        public string SubDomain { get; set; }
    }

    public class GetResourceDocumentationsResponse
    {
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
            var response = new GetResourceDocumentationsResponse
            {
                Files = resourceDocFiles.Files.Select(t => new GetResourceDocumentationsFile
                {
                    JsonPath = t.JsonPath,
                    MdPath = t.MdPath,
                    Name = t.Name,
                    Domain = t.Domain,
                    SubDomain = t.SubDomain
                })
            };
            return response;
        }
    }
}

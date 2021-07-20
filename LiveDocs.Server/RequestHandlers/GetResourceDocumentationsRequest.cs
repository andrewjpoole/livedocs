using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiveDocs.Server.config;
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
    }

    public class GetResourceDocumentationsResponse
    {
        public IEnumerable<GetResourceDocumentationsFile> Files { get; init; }
    }

    public class GetResourceDocumentationsRequestHandler : IRequestHandler<GetResourceDocumentationsRequest, GetResourceDocumentationsResponse>
    {
        private readonly IOptions<StronglyTypedConfig.LiveDocs> _liveDocsOptions;

        public GetResourceDocumentationsRequestHandler(IOptions<StronglyTypedConfig.LiveDocs> liveDocsOptions)
        {
            _liveDocsOptions = liveDocsOptions;
        }

        public async Task<GetResourceDocumentationsResponse> Handle(GetResourceDocumentationsRequest request, CancellationToken cancellationToken)
        {
            var response = new GetResourceDocumentationsResponse
            {
                Files = _liveDocsOptions.Value.Files.Select(t => new GetResourceDocumentationsFile
                {
                    JsonPath = t.JsonPath,
                    MdPath = t.MdPath,
                    Name = t.Name
                })
            };
            return response;
        }
    }
}

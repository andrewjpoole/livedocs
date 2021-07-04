using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiveDocs.Server.Services;
using MediatR;

namespace LiveDocs.Server.RequestHandlers
{
    public class GetLiveDocsRequest : IRequest<GetLiveDocsResponse>
    {
        public string ResourceName { get; init; }
    }

    public class GetLiveDocsResponse
    {
        public string Markdown { get; init; }
    }

    public class GetLiveDocsRequestHandler : IRequestHandler<GetLiveDocsRequest, GetLiveDocsResponse>
    {
        private readonly IAggregatorBackgroundService _aggregatorBackgroundService;

        public GetLiveDocsRequestHandler(IAggregatorBackgroundService aggregatorBackgroundService)
        {
            _aggregatorBackgroundService = aggregatorBackgroundService;
        }

        public async Task<GetLiveDocsResponse> Handle(GetLiveDocsRequest request, CancellationToken cancellationToken)
        {
            var response = new GetLiveDocsResponse
            {
                Markdown = _aggregatorBackgroundService.GetLatestMarkdown(request.ResourceName)
            };
            return response;
        }
    }
}

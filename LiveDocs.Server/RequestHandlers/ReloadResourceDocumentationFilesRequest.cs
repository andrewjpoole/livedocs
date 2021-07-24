using System.Threading;
using System.Threading.Tasks;
using LiveDocs.Server.Services;
using MediatR;

namespace LiveDocs.Server.RequestHandlers
{
    public class ReloadResourceDocumentationFilesRequest : IRequest, IRequest<ReloadResourceDocumentationFilesResponse>
    {
    }

    public class ReloadResourceDocumentationFilesResponse
    {
    }

    public class ReloadResourceDocumentationFilesRequestHandler : IRequestHandler<ReloadResourceDocumentationFilesRequest, ReloadResourceDocumentationFilesResponse>
    {
        private readonly IAggregatorBackgroundService _aggregatorBackgroundService;

        public ReloadResourceDocumentationFilesRequestHandler(IAggregatorBackgroundService aggregatorBackgroundService)
        {
            _aggregatorBackgroundService = aggregatorBackgroundService;
        }

        public async Task<ReloadResourceDocumentationFilesResponse> Handle(ReloadResourceDocumentationFilesRequest request, CancellationToken cancellationToken)
        {
            _aggregatorBackgroundService.ReloadResourceDocumentationFiles();
            return new ReloadResourceDocumentationFilesResponse();
        }
    }
}
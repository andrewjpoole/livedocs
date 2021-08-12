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
        private readonly IMarkdownReplacementAggregatorBackgroundService _markdownReplacementAggregatorBackgroundService;

        public ReloadResourceDocumentationFilesRequestHandler(IMarkdownReplacementAggregatorBackgroundService markdownReplacementAggregatorBackgroundService)
        {
            _markdownReplacementAggregatorBackgroundService = markdownReplacementAggregatorBackgroundService;
        }

        public async Task<ReloadResourceDocumentationFilesResponse> Handle(ReloadResourceDocumentationFilesRequest request, CancellationToken cancellationToken)
        {
            await _markdownReplacementAggregatorBackgroundService.ReloadResourceDocumentationFiles();
            return new ReloadResourceDocumentationFilesResponse();
        }
    }
}
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
        private readonly IMarkdownReplacementAggregator _markdownReplacementAggregator;

        public ReloadResourceDocumentationFilesRequestHandler(IMarkdownReplacementAggregator markdownReplacementAggregator)
        {
            _markdownReplacementAggregator = markdownReplacementAggregator;
        }

        public async Task<ReloadResourceDocumentationFilesResponse> Handle(ReloadResourceDocumentationFilesRequest request, CancellationToken cancellationToken)
        {
            _markdownReplacementAggregator.ReloadResourceDocumentationFiles();
            return new ReloadResourceDocumentationFilesResponse();
        }
    }
}
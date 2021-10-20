using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LiveDocs.Server.RequestHandlers
{
    public class GetStatsRequest : IRequest<GetStatsResponse>
    {
    }

    public class GetStatsResponse
    {
        public object Stats { get; }

        public GetStatsResponse(object stats)
        {
            Stats = stats;
        }
    }

    public class GetStatsRequestHandler : IRequestHandler<GetStatsRequest, GetStatsResponse>
    {
        private readonly IInMemoryReplacementCacheBackgroundTaskQueueStats _inMemoryReplacementCacheBackgroundTaskQueueStats;
        private readonly IResourceDocumentationStats _resourceDocumentationStats;
        private readonly IConnectedClientStats _connectedClientStats;

        public GetStatsRequestHandler(IInMemoryReplacementCacheBackgroundTaskQueueStats inMemoryReplacementCacheBackgroundTaskQueueStats, IResourceDocumentationStats resourceDocumentationStats, IConnectedClientStats connectedClientStats)
        {
            _inMemoryReplacementCacheBackgroundTaskQueueStats = inMemoryReplacementCacheBackgroundTaskQueueStats;
            _resourceDocumentationStats = resourceDocumentationStats;
            _connectedClientStats = connectedClientStats;
        }

        public async Task<GetStatsResponse> Handle(GetStatsRequest request, CancellationToken cancellationToken)
        {
            return new GetStatsResponse(new
            {
                ConnectedClientsStats = await _connectedClientStats.GetStats(),
                ReplacementCacheBackgroundTaskQueueStats = await _inMemoryReplacementCacheBackgroundTaskQueueStats.GetStats(),
                ResourceDocumentationStats = await _resourceDocumentationStats.GetStats()
            });
        }
    }

    public interface IStatsProvider
    {
        Task<object> GetStats();
    }

    public interface IInMemoryReplacementCacheBackgroundTaskQueueStats : IStatsProvider
    {
    }

    public interface IResourceDocumentationStats : IStatsProvider
    {
    }

    public interface IConnectedClientStats : IStatsProvider
    {
    }

}
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LiveDocs.Server.Services
{
    public interface IBackgroundTaskQueue
    {
        ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);

        ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
            CancellationToken cancellationToken);

        int ItemCount { get; }
    }
}
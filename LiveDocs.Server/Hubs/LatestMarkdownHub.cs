using System;
using System.Threading.Tasks;
using LiveDocs.Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace LiveDocs.Server.Hubs
{
    public class LatestMarkdownHub : Hub
    {
        private readonly ILogger<LatestMarkdownHub> _logger;
        private readonly IHubGroupTracker _hubGroupTracker;
        private readonly IMarkdownReplacementAggregatorBackgroundService _markdownReplacementAggregatorBackgroundService;


        public LatestMarkdownHub(ILogger<LatestMarkdownHub> logger, IHubGroupTracker hubGroupTracker, IMarkdownReplacementAggregatorBackgroundService markdownReplacementAggregatorBackgroundService)
        {
            _logger = logger;
            _hubGroupTracker = hubGroupTracker;
            _markdownReplacementAggregatorBackgroundService = markdownReplacementAggregatorBackgroundService;
        }

        public async Task SelectResource(string resource)
        {
            // A client connection is requesting markdown for a particular resource, assign them to the appropriate group etc
            // ToDo validate the resource name
            _logger.LogInformation($"SignalR client {Context.ConnectionId} requesting markdown for {resource}");

            await Groups.AddToGroupAsync(Context.ConnectionId, resource);
            _hubGroupTracker.MoveConnectionIdToGroup(Context.ConnectionId, resource);

            await _markdownReplacementAggregatorBackgroundService.SendLatestMarkDownForNewGroupMember(resource, Context.ConnectionId);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _hubGroupTracker.RemoveConnectionIdFromAllGroups(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}

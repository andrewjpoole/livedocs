using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace LiveDocs.Server.Hubs
{
    public class LatestMarkdownHub : Hub
    {
        private readonly ILogger<LatestMarkdownHub> _logger;

        public LatestMarkdownHub(ILogger<LatestMarkdownHub> logger)
        {
            _logger = logger;
        }

        public async Task SelectResource(string resource)
        {
            // A client is requesting markdown for a particular resource, assign them to the appropriate group etc
            // ToDo validate the resource name
            _logger.LogInformation($"SignalR client {Context.ConnectionId} requesting markdown for {resource}");
            var client = Context.ConnectionId;
            await Groups.AddToGroupAsync(client, resource);
        }
    }
}

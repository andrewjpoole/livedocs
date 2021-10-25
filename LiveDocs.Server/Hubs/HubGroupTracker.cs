using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveDocs.Server.RequestHandlers;
using Microsoft.AspNetCore.SignalR;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LiveDocs.Server.Hubs
{
    public class HubGroupTracker : IHubGroupTracker, IConnectedClientStats
    {
        private Dictionary<string, Dictionary<string, ConnectedClient>> _groupMembership = new();

        public void MoveConnectionIdToGroup(HubCallerContext context, string groupName)
        {
            // a single connection (i.e. a browser tab) should only be in one group at a time
            // a user may have multiple tabs open, but each will have its own connection Id
            RemoveConnectionIdFromAllGroups(context.ConnectionId);

            var connectedClient = new ConnectedClient(context.ConnectionId, context.User?.Identity?.Name ?? "Name is null", context.UserIdentifier ?? "UserId is null");

            if (!_groupMembership.ContainsKey(groupName))
            {
                _groupMembership.Add(groupName, new Dictionary<string, ConnectedClient>{{ context.ConnectionId, connectedClient } });
                return;
            }

            _groupMembership[groupName].Add(context.ConnectionId, connectedClient);
        }

        public void RemoveConnectionIdFromAllGroups(string connectionId)
        {
            foreach (var groupMembers in _groupMembership.Values.Where(groupMembers => groupMembers.ContainsKey(connectionId)))
            {
                groupMembers.Remove(connectionId);
            }
        }

        public bool GroupHasConnections(string groupName)
        {
            return _groupMembership.ContainsKey(groupName) && _groupMembership[groupName].Any();
        }

        public async Task<object> GetStats()
        {
            return new
            {
                ConnectedClientGroups = _groupMembership.Select(x => new
                {
                    Name = x.Key,
                    ConnectedClients = x.Value.Values.Select(y => new
                    {
                        y.ConnectionId,
                        y.UserIdentifier,
                        y.Name,
                        y.JoinedGroupAt
                    })
                })
            };
        }
    }

    public class ConnectedClient
    {
        public string ConnectionId { get; }
        public string UserIdentifier { get; }
        public string Name { get; }
        public DateTime JoinedGroupAt { get; }

        public ConnectedClient(string connectionId, string name, string userIdentifier)
        {
            ConnectionId = connectionId;
            Name = name;
            UserIdentifier = userIdentifier;
            JoinedGroupAt = DateTime.Now;
        }
    }
}
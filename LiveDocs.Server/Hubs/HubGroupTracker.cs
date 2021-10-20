using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveDocs.Server.RequestHandlers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LiveDocs.Server.Hubs
{
    public class HubGroupTracker : IHubGroupTracker, IConnectedClientStats
    {
        private Dictionary<string, Dictionary<string, ConnectedClient>> _groupMembership = new();

        public void MoveConnectionIdToGroup(string connectionId, string userIdentifier, string groupName)
        {
            // a single connection (i.e. a browser tab) should only be in one group at a time
            // a user may have multiple tabs open, but each will have its own connection Id
            RemoveConnectionIdFromAllGroups(connectionId);

            if (!_groupMembership.ContainsKey(groupName))
            {
                _groupMembership.Add(groupName, new Dictionary<string, ConnectedClient>{{ connectionId, new ConnectedClient(connectionId, userIdentifier) }});
                return;
            }

            _groupMembership[groupName].Add(connectionId, new ConnectedClient(connectionId, userIdentifier));
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
        public DateTime JoinedGroupAt { get; }

        public ConnectedClient(string connectionId, string userIdentifier)
        {
            ConnectionId = connectionId;
            UserIdentifier = userIdentifier;
            JoinedGroupAt = DateTime.Now;
        }
    }
}
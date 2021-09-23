using System.Collections.Generic;
using System.Linq;

namespace LiveDocs.Server.Hubs
{
    public class HubGroupTracker : IHubGroupTracker
    {
        // todo add get group connection stats method and expose via mediatrendpoint?

        private Dictionary<string, List<string>> _groupMembership = new();

        public void MoveConnectionIdToGroup(string connectionId, string groupName)
        {
            // a single connection (i.e. a browser tab) should only be in one group at a time
            // a user may have multiple tabs open, but each will have its own connection Id
            RemoveConnectionIdFromAllGroups(connectionId);

            if (!_groupMembership.ContainsKey(groupName))
            {
                _groupMembership.Add(groupName, new List<string>{ connectionId });
                return;
            }

            _groupMembership[groupName].Add(connectionId);
        }

        public void RemoveConnectionIdFromAllGroups(string connectionId)
        {
            foreach (var groupMembers in _groupMembership.Values.Where(groupMembers => groupMembers.Contains(connectionId)))
            {
                groupMembers.Remove(connectionId);
            }
        }

        public bool GroupHasConnections(string groupName)
        {
            return _groupMembership.ContainsKey(groupName) && _groupMembership[groupName].Any();
        }
    }
}
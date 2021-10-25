using Microsoft.AspNetCore.SignalR;

namespace LiveDocs.Server.Hubs
{
    public interface IHubGroupTracker
    {
        void MoveConnectionIdToGroup(HubCallerContext context, string groupName);

        void RemoveConnectionIdFromAllGroups(string connectionId);

        bool GroupHasConnections(string groupName);
    }
}
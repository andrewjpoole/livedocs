namespace LiveDocs.Server.Hubs
{
    public interface IHubGroupTracker
    {
        void MoveConnectionIdToGroup(string connectionId, string groupName);

        void RemoveConnectionIdFromAllGroups(string connectionId);

        bool GroupHasConnections(string groupName);
    }
}
namespace LiveDocs.Server.Services
{
    public interface IReplacementCache
    {
        void RegisterReplacement(string name, string instruction, string timeToLive, bool replaceIfKeyExists);
        string FetchCurrentReplacementValue(string name, string instruction, bool waitForNewValueIfExpired);
        void ClearCache();
    }
}
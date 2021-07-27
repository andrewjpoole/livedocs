using System.Threading.Tasks;

namespace LiveDocs.Server.Services
{
    public interface IReplacementCache
    {
        void RegisterReplacement(string name, string instruction, string timeToLive, bool replaceIfKeyExists);
        Task<(string Name, string Data)> FetchCurrentReplacementValue(string name, string instruction, bool waitForNewValueIfExpired);
        void ClearCache();
    }
}
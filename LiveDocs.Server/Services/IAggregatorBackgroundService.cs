using System.Threading.Tasks;

namespace LiveDocs.Server.Services
{
    public interface IAggregatorBackgroundService
    {
        Task<string> GetLatestMarkdown(string resourceName);
        void ReloadResourceDocumentationFiles();
    }
}
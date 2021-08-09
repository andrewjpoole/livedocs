using System.Threading.Tasks;

namespace LiveDocs.Server.Services
{
    public interface IMarkdownReplacementAggregator
    {
        Task<string> GetLatestMarkdown(string resourceName);
        void ReloadResourceDocumentationFiles();
    }
}
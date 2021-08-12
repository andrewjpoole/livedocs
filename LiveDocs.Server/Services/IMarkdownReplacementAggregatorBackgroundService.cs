using System.Threading.Tasks;

namespace LiveDocs.Server.Services
{
    public interface IMarkdownReplacementAggregatorBackgroundService
    {
        Task ReloadResourceDocumentationFiles();
    }
}
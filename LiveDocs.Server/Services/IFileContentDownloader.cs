using System.Threading.Tasks;

namespace LiveDocs.Server.Services
{
    public interface IFileContentDownloader
    {
        Task<string> Fetch(string url);
    }
}
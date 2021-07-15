using System.Threading.Tasks;

namespace LiveDocs.Server.Services
{
    public interface IAzureRMApiClient
    {
        Task<T> Query<T>(string uri);
    }
}
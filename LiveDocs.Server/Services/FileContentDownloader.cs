using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LiveDocs.Server.Services
{
    public class FileContentDownloader : IFileContentDownloader
    {
        private readonly IHttpClientFactory _clientFactory;

        public FileContentDownloader(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<string> Fetch(string url)
        {
            try
            {
                var client = _clientFactory.CreateClient(url.StartsWith("https://dev.azure.com") ? "AzureDevOpsClient" : "PublicUrlClient");

                return await client.GetStringAsync(url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }
}
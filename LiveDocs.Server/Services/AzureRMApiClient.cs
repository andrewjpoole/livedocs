using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LiveDocs.Server.Services
{
    public class AzureRMApiClient : IAzureRMApiClient
    {
        private readonly IAzureIAMTokenFetcher _tokenFetcher;
        private readonly string _azureResourceManagementApiBaseUri;

        public AzureRMApiClient(IAzureIAMTokenFetcher tokenFetcher, IConfiguration configuration)
        {
            _tokenFetcher = tokenFetcher;
            _azureResourceManagementApiBaseUri =
                configuration.GetSection("livedocs")["azureResourceManagementApiBaseUri"];
        }

        public async Task<T> Query<T>(string uri)
        {
            if (_tokenFetcher?.Token is null || _tokenFetcher.Token.ValidTo < DateTime.UtcNow)
            {
                await _tokenFetcher.Fetch();
            }

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_azureResourceManagementApiBaseUri);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", _tokenFetcher.BearerHeaderValue);
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Unable to query Azure Resource Management API. {response.StatusCode} {response.Content.ReadAsStreamAsync()}");

            return await JsonSerializer.DeserializeAsync<T>(response.Content.ReadAsStream(), new JsonSerializerOptions{PropertyNameCaseInsensitive = true});
        }
    }
}
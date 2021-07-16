using System;
using System.Data;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LiveDocs.Server.config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LiveDocs.Server.Services
{
    public class AzureRMApiClient : IAzureRMApiClient
    {
        private readonly IAzureIAMTokenFetcher _tokenFetcher;
        private readonly string _azureResourceManagementApiBaseUri;
        private const string AuthHeaderKeyName = "Authorization";

        public AzureRMApiClient(IAzureIAMTokenFetcher tokenFetcher, IOptions<StronglyTypedConfig.LiveDocs> liveDocsOptions)
        {
            _tokenFetcher = tokenFetcher;
            _azureResourceManagementApiBaseUri = liveDocsOptions.Value.AzureResourceManagementApiBaseUri;
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
            request.Headers.Add(AuthHeaderKeyName, _tokenFetcher.BearerHeaderValue);
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Unable to query Azure Resource Management API. {response.StatusCode} {response.Content.ReadAsStreamAsync()}");

            return await JsonSerializer.DeserializeAsync<T>(response.Content.ReadAsStream(), new JsonSerializerOptions{PropertyNameCaseInsensitive = true});
        }
    }
}
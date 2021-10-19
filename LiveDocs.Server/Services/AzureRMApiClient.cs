using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LiveDocs.Server.Services
{
    public class AzureRMApiClient : IAzureRMApiClient
    {
        private readonly IAzureIAMTokenFetcher _tokenFetcher;
        private readonly IHttpClientFactory _clientFactory;
        private const string AuthHeaderKeyName = "Authorization";

        public AzureRMApiClient(IAzureIAMTokenFetcher tokenFetcher, IHttpClientFactory clientFactory)
        {
            _tokenFetcher = tokenFetcher ?? throw new ArgumentNullException(nameof(tokenFetcher));
            _clientFactory = clientFactory;
        }

        public async Task<T> Query<T>(string uri)
        {
            if (_tokenFetcher?.Token is null || _tokenFetcher.Token.ValidTo < DateTime.UtcNow)
            {
                await _tokenFetcher!.Fetch();
            }

            using var httpClient = _clientFactory.CreateClient("AzureRMClient");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add(AuthHeaderKeyName, _tokenFetcher.BearerHeaderValue);
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Unable to query Azure Resource Management API. {response.StatusCode} {response.Content.ReadAsStreamAsync()}");

            var result = await JsonSerializer.DeserializeAsync<T>(response.Content.ReadAsStream(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return result ?? throw new Exception("Unable to deserialize the response");
        }
    }
}
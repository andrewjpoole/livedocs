using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LiveDocs.Server.config;
using LiveDocs.Server.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveDocs.Server.Services
{
    public class AzureIAMTokenFetcher : IAzureIAMTokenFetcher
    {
        private readonly IOptions<StronglyTypedConfig.AzureAd> _azureAdOptions;
        private readonly ILogger<AzureIAMTokenFetcher> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private const string GrantTypeHeaderKeyName = "grant_type";
        private const string GrantTypeHeaderKeyValue = "client_credentials";
        private const string ClientIdHeaderKeyName = "client_id";
        private const string ClientSecretHeaderKeyName = "client_secret";
        private const string ResourceHeaderKeyName = "resource";
        private const string AzureTokenUriPart = "/oauth2/token";

        public string BearerHeaderValue => $"Bearer {Token?.RawData}";
        public JwtSecurityToken? Token { get; private set; }

        public AzureIAMTokenFetcher(IOptions<StronglyTypedConfig.AzureAd> azureAdOptions, ILogger<AzureIAMTokenFetcher> logger, IHttpClientFactory clientFactory)
        {
            _azureAdOptions = azureAdOptions;
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public async Task Fetch()
        {
            try
            {
                using var httpClient = _clientFactory.CreateClient("AzureIAMClient");
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_azureAdOptions.Value.TenantId}{AzureTokenUriPart}");
                var formContents = new List<KeyValuePair<string?, string?>>
                {
                    new(GrantTypeHeaderKeyName, GrantTypeHeaderKeyValue),
                    new(ClientIdHeaderKeyName, _azureAdOptions.Value.ClientId),
                    new(ClientSecretHeaderKeyName, _azureAdOptions.Value.ClientSecret),
                    new(ResourceHeaderKeyName, _azureAdOptions.Value.Resource)
                };
                request.Content = new FormUrlEncodedContent(formContents);

                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new ApplicationException($"Unable to obtain jwt from Azure AD. {response.StatusCode} {response.Content.ReadAsStringAsync()}");
                }

                var tokenResponse = await JsonSerializer.DeserializeAsync<OathTokenResponse>(response.Content.ReadAsStream());
                Token = new JwtSecurityToken(tokenResponse?.access_token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error thrown while fetching token for the service principal with ClientId:{_azureAdOptions.Value.ClientId} {e}");
                throw;
            }
            
        }
    }
}
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using LiveDocs.Server.Models;
using Microsoft.Extensions.Configuration;

namespace LiveDocs.Server.Services
{
    public class AzureIAMTokenFetcher : IAzureIAMTokenFetcher
    {
        private readonly IConfiguration _azureAdConfiguration;

        public string BearerHeaderValue => $"Bearer {Token.RawData}";
        public JwtSecurityToken Token { get; private set; }

        public AzureIAMTokenFetcher(IConfiguration configuration)
        {
            _azureAdConfiguration = configuration.GetSection("AzureAD");
        }

        public async Task Fetch()
        {
            try
            {
                using var httpClient = new HttpClient();
                var uriString = $"https://login.microsoftonline.com";
                httpClient.BaseAddress = new Uri(uriString);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_azureAdConfiguration["tenantId"]}/oauth2/token");
                var formContents = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "client_credentials"),
                    new("client_id", _azureAdConfiguration["clientId"]),
                    new("client_secret", _azureAdConfiguration["clientSecret"]),
                    new("resource", _azureAdConfiguration["resource"])
                };
                request.Content = new FormUrlEncodedContent(formContents);
                //request.Content.Headers.ContentType = new MediaTypeHeaderValue("x-www-form-urlencoded"); //Headers.Add("Content-Type", "x-www-form-urlencoded");

                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new ApplicationException($"Unable to obtain jwt from Azure AD. {response.StatusCode} {response.Content.ReadAsStringAsync()}");
                }

                var tokenResponse = await JsonSerializer.DeserializeAsync<OathTokenResponse>(response.Content.ReadAsStream());
                Token = new JwtSecurityToken(tokenResponse.access_token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }
}
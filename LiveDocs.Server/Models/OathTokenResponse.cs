// ReSharper disable InconsistentNaming
namespace LiveDocs.Server.Models
{
    public class OathTokenResponse
    {
        public OathTokenResponse(string token_type, string expires_in, string ext_expires_in, string expires_on, string not_before, string resource, string access_token)
        {
            this.token_type = token_type;
            this.expires_in = expires_in;
            this.ext_expires_in = ext_expires_in;
            this.expires_on = expires_on;
            this.not_before = not_before;
            this.resource = resource;
            this.access_token = access_token;
        }

        public string token_type { get; }
        public string expires_in { get; }
        public string ext_expires_in { get; }
        public string expires_on { get; }
        public string not_before { get; }
        public string resource { get; }
        public string access_token { get; }
    }
}
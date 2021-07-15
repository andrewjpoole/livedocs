using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace LiveDocs.Server.Services
{
    public interface IAzureIAMTokenFetcher
    {
        string BearerHeaderValue { get; }
        JwtSecurityToken Token { get; }
        Task Fetch();
    }
}
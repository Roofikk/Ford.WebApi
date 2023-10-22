using Ford.WebApi.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Pathnostics.Web.Extensions;
using System.Text;

namespace Ford.WebApi.Services.Identity;

public class TokenService : ITokenService
{
    private readonly IConfiguration configuration;

    public TokenService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public string GenerateToken(User user, List<IdentityRole<long>> roles, TimeSpan tokenLifeTime)
    {
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);

        return user.CreateClaims(roles).CreateToken(issuer, audience, key, tokenLifeTime);
    }
}

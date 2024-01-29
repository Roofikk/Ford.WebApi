using Ford.WebApi.Data.Entities;
using Ford.WebApi.Extensions.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ford.WebApi.Services.Identity;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<User> userManager;

    public TokenService(IOptions<JwtSettings> jwtSettings, UserManager<User> userManager)
    {
        _jwtSettings = jwtSettings.Value;
        this.userManager = userManager;
    }

    public string GenerateToken(User user, List<IdentityRole<long>> roles, TimeSpan tokenLifeTime)
    {
        var issuer = _jwtSettings.Issuer;
        var audience = _jwtSettings.Audience;
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

        return user.CreateClaims(roles).CreateToken(issuer, audience, key, tokenLifeTime);
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            ValidateLifetime = true
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }

    public async Task<User?> GetUserByToken(string? jwtToken)
    {
        ClaimsPrincipal? principal = GetPrincipalFromToken(jwtToken.Replace("Bearer ", string.Empty));

        if (principal == null)
        {
            return null;
        }

        string? userName = principal.Identity!.Name;
        User? user = await userManager.FindByNameAsync(userName);

        if (user is null)
        {
            return null;
        }
        else
        {
            return user;
        }
    } 
}

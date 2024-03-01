using Ford.WebApi.Data.Entities;
using Ford.WebApi.Extensions.Authentication;
using Ford.WebApi.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Ford.WebApi.Services.Identity;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<User> userManager;

    private static readonly TimeSpan tokenLifeTime = TimeSpan.FromHours(2);

    public TokenService(IOptions<JwtSettings> jwtSettings, UserManager<User> userManager)
    {
        _jwtSettings = jwtSettings.Value;
        this.userManager = userManager;
    }

    public async Task<Token> GenerateTokenAsync(User user)
    {
        var issuer = _jwtSettings.Issuer;
        var audience = _jwtSettings.Audience;
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);
        var roles = await userManager.GetRolesAsync(user);

        List<Claim> claims = new()
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new("roles", String.Join(',', roles))
        };

        var claimsIdentity = new ClaimsIdentity(claims);

        var securityToken = new SecurityTokenDescriptor()
        {
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.Add(tokenLifeTime),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.WriteToken(tokenHandler.CreateToken(securityToken));
        var refreshToken = GenerateRefreshToken();

        return new Token
        {
            JwtToken= jwtToken,
            RefreshToken = refreshToken,
            ExpiredDate = DateTime.Now.AddDays(14)
        };
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Key)),
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken)
        {
            throw new SecurityTokenException("Invalid Token");
        }

        return principal;
    }

    public string? GetUserId(ClaimsPrincipal claimsPrincipal)
    {
        Claim? claimUserId = claimsPrincipal.Claims
            .SingleOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId || c.Type == ClaimTypes.NameIdentifier);

        if (claimUserId is null)
        {
            return null;
        }

        return claimUserId.Value;
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}

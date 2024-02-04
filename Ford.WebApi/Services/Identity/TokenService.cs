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

    public async Task<string> GenerateToken(User user, TimeSpan tokenLifeTime)
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

        return jwtToken;
    }

    public async Task<User?> GetUserByPrincipal(ClaimsPrincipal userPrincipal)
    {
        //ClaimsPrincipal? principal = GetPrincipalFromToken(jwtToken.Replace("Bearer ", string.Empty));
        Claim? claimUserId = userPrincipal.Claims
            .SingleOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId || c.Type == ClaimTypes.NameIdentifier);

        if (claimUserId is null)
        {
            return null;
        }

        User? user = await userManager.FindByIdAsync(claimUserId.Value);

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

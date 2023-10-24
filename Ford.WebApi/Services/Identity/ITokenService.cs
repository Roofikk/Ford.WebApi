using Ford.WebApi.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Ford.WebApi.Services.Identity
{
    public interface ITokenService
    {
        public string GenerateToken(User user, List<IdentityRole<long>> roles, TimeSpan tokenLifeTime);
        public ClaimsPrincipal? GetPrincipalFromToken(string? token);
        public Task<User?> GetUserByToken(string? jwtToken);
    }
}

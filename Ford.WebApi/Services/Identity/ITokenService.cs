using Ford.WebApi.Data.Entities;
using System.Security.Claims;

namespace Ford.WebApi.Services.Identity
{
    public interface ITokenService
    {
        public Task<string> GenerateToken(User user, TimeSpan tokenLifeTime);
        public Task<User?> GetUserByPrincipal(ClaimsPrincipal claimsPrincipal);
        public string? GetUserId(ClaimsPrincipal claimsPrincipal);
    }
}

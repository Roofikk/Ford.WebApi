using Ford.WebApi.Data.Entities;
using Ford.WebApi.Models.Identity;
using System.Security.Claims;

namespace Ford.WebApi.Services.Identity
{
    public interface ITokenService
    {
        public Task<Token> GenerateTokenAsync(User user);
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        public string? GetUserId(ClaimsPrincipal claimsPrincipal);
    }
}

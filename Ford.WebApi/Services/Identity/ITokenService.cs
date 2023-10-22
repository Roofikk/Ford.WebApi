using Ford.WebApi.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Ford.WebApi.Services.Identity
{
    public interface ITokenService
    {
        public string GenerateToken(User user, List<IdentityRole<long>> roles, TimeSpan tokenLifeTime);
    }
}

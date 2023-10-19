using System.Security.Claims;

namespace Ford.WebApi;

public class TokenGenerationRequest
{
    public string UserId { get; set; } = null!;
    public string Login { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? Email { get; set; }
}

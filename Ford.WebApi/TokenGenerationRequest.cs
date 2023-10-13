using System.Security.Claims;

namespace Ford.WebApi;

public class TokenGenerationRequest
{
    public string UserId { get; set; }
    public string Login { get; set; }
    public string? Email { get; set; }
}

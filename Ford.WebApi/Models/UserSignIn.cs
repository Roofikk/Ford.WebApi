using System.Security.Claims;

namespace Ford.WebApi.Models;

public class UserSignIn
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Email { get; set; }
}

namespace Ford.WebApi.Models;

public class AuthResponse
{
    public string Login { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
}

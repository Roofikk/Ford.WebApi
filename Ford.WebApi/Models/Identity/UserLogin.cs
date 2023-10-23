using System.Security.Claims;

namespace Ford.WebApi.Models.Identity;

public class UserLogin
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
}

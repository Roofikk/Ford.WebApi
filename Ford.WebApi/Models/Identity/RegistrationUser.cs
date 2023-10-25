namespace Ford.WebApi.Models.Identity;

public class RegistrationUser
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
}

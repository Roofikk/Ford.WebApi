namespace Ford.WebApi.Models.Identity;

public class RequestChangePassword
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

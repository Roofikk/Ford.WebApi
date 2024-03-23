namespace Ford.WebApi.Dtos.User;

public class UserGettingDto : MinimalUserDto
{
    public string Login { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public DateOnly? BirthDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
}

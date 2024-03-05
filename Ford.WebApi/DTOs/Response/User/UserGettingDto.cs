namespace Ford.WebApi.Dtos.User;

public class UserGettingDto
{
    public long UserId { get; set; }
    public string Login { get; set; } = null!;
    public string? Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
}

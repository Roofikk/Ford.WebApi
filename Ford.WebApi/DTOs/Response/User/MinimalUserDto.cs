namespace Ford.WebApi.Dtos.User;

public class MinimalUserDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public DateOnly? BirthDate { get; set; }
    public DateTime CreationDate { get; set; }
}

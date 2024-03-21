namespace Ford.WebApi.Dtos.User;

public class MinimalUserDto
{
    public long UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string? City { get; set; }
    public DateTime CreationDate { get; set; }
}

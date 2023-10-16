namespace Ford.WebApi.Dtos.User;

public class UserGettingDto
{
    public string UserId { get; set; } = null!;
    public string Login {  get; set; } = null!;
    public string? Email { get; set; }
    public string Name { get; set; } = null!;
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
}

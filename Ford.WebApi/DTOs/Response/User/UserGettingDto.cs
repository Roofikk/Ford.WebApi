namespace Ford.WebApi.Dtos.User;

public class UserGettingDto : MinimalUserDto
{
    public string? Email { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
}

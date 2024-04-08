namespace Ford.WebApi.Dtos.User;

public class UserDto : MinimalUserDto
{
    public string? Email { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
}

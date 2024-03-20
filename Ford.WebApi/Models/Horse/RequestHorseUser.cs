namespace Ford.WebApi.Models.Horse;

public class RequestHorseUser
{
    public long UserId { get; set; }
    public string AccessRole { get; set; } = null!;
    public bool IsOwner { get; set; }
}

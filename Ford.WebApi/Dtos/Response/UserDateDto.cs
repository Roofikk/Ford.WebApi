using Ford.WebApi.Dtos.Horse;

namespace Ford.WebApi.Dtos.Response;

public class UserDateDto
{
    public DateTime Date { get; set; }
    public HorseUserDto User { get; set; } = null!;
}

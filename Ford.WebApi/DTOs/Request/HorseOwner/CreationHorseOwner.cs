using Ford.WebApi.Data.Entities;

namespace Ford.WebApi.Dtos.Request;

public class CreationHorseOwner
{
    public long UserId { get; set; }
    public long HorseId { get; set; }
    public string? OwnerAccessRole { get; set; }
}

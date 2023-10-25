using Ford.WebApi.Data.Entities;

namespace Ford.WebApi.Models.Horse;

public class RequestUpdateHorseOwners
{
    public long HorseId { get; set; }
    public IEnumerable<RequestHorseOwner> HorseOwners { get; set; } = null!;
}

public class RequestHorseOwner
{
    public long UserId { get; set; }
    public string RuleAccess { get; set; } = null!;
    public User? User { get; set; }
}

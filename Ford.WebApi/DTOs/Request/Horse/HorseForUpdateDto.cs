using Ford.WebApi.Models.Horse;

namespace Ford.WebApi.Dtos.Horse;

public class HorseForUpdateDto
{
    public long HorseId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Sex { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }

    public IEnumerable<RequestHorseOwner> Owners { get; set; } = null!;
}

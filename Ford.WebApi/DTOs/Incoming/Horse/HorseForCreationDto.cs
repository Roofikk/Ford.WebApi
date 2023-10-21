using Ford.EntityModels.Models;

namespace Ford.WebApi.Dtos.Horse;

public class HorseForCreationDto
{
    public string Name { get; set; } = null!;
    public DateTime? BirthDate { get; set; }
    public string? Sex { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public IEnumerable<HorseOwner> HorseOwners { get; set; } = null!;
}

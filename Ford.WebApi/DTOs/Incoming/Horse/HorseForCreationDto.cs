using Ford.WebApi.Data.Entities;
using Ford.WebApi.Models.Horse;

namespace Ford.WebApi.Dtos.Horse;

public class HorseForCreationDto
{
    public string Name { get; set; } = null!;
    public DateTime? BirthDate { get; set; }
    public string? Sex { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public IEnumerable<RequestHorseOwner>? HorseOwners { get; set; }
}

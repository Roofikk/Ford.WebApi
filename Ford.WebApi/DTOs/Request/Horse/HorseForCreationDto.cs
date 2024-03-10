using Ford.WebApi.Dtos.Request;
using Ford.WebApi.Models.Horse;

namespace Ford.WebApi.Dtos.Horse;

public class HorseForCreationDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Sex { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerPhoneNumber { get; set; }
    public ICollection<RequestUpdateSaveDto> Saves { get; set; } = [];
    public ICollection<RequestHorseOwner> Users { get; set; } = [];
}

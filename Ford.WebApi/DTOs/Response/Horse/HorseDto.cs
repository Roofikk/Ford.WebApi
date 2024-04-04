using Ford.WebApi.Dtos.Response;

namespace Ford.WebApi.Dtos.Horse;

public class HorseDto : IStorageData
{
    public long HorseId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Sex { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerPhoneNumber { get; set; }
    public HorseUserDto Self { get; set; } = null!;
    public ICollection<HorseUserDto> Users { get; set; } = [];
    public ICollection<SaveDto> Saves { get; set; } = [];
}

public class HorseUserDto
{
    public long UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsOwner { get; set; }
    public string AccessRole { get; set; } = null!;
}
﻿using Ford.WebApi.Dtos.Response;

namespace Ford.WebApi.Dtos.Horse;

public class HorseRetrievingDto
{
    public long HorseId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Sex { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public ICollection<OwnerDto> Users { get; set; } = null!;
    public ICollection<ResponseSaveDto> Saves { get; set; } = null!;
}

public class OwnerDto
{
    public long Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string OwnerAccessRole { get; set; } = null!;
}
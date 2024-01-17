using Ford.WebApi.Data.Entities;

namespace Ford.WebApi.Dtos.Horse
{
    public class HorseRetrievingDto
    {
        public long HorseId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Sex { get; set; } = Data.Entities.Sex.None.ToString();
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public DateTime CreationDate { get; set; }
        public IEnumerable<OwnerDto>? Users { get; set; }
    }

    public class OwnerDto
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }
        public string RuleAccess { get; set; } = null!;
    }
}

using Ford.Common.EntityModels.Models;

namespace Ford.WebApi.DTOs.Outgoing.Horse
{
    public class HorseRetrievingDto
    {
        public long HorseId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public string Sex { get; set; } = Common.EntityModels.Models.Sex.None.ToString();
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public DateTime CreationDate { get; set; }
        public IEnumerable<Save>? Saves { get; set; }
        public IEnumerable<HorseUserDto>? Users { get; set; }
    }

    public class HorseUserDto
    {
        public string Id { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? LastName { get; set; }
    }
}

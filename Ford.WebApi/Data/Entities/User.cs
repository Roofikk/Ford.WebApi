using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Ford.WebApi.Data.Entities
{
    public partial class User : IdentityUser<long>
    {
        public User()
        {
            HorseOwners = new HashSet<HorseOwner>();
        }

        [Column(TypeName = "nvarchar(20)")]
        public string FirstName { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public string? LastName { get; set; }
        [Column(TypeName = "nvarchar(25)")]
        public string? City { get; set; }
        [Column(TypeName = "nvarchar(25)")]
        public string? Region { get; set; }
        [Column(TypeName = "nvarchar(25)")]
        public string? Country { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? BirthDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime CreationDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime LastUpdatedDate { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<HorseOwner> HorseOwners { get; set; }
        public virtual ICollection<Save> Saves { get; set; }
    }
}

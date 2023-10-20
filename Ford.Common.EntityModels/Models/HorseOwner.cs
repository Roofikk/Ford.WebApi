using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ford.EntityModels.Models
{
    public partial class HorseOwner
    {
        [Key]
        public long HorseId { get; set; }
        [Key]
        [Column(TypeName = "nvarchar(40)")]
        public string UserId { get; set; } = null!;
        [Column(TypeName = "nvarchar(8)")]
        public string RuleAccess { get; set; } = null!;

        [ForeignKey("UserId")]
        [InverseProperty("HorseOwners")]
        public virtual User User { get; set; } = null!;
        [ForeignKey("HorseId")]
        [InverseProperty("HorseOwners")]
        public virtual Horse Horse { get; set; } = null!;
    }
}

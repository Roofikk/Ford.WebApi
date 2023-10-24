using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ford.WebApi.Data.Entities
{
    public partial class HorseOwner
    {
        [Key]
        public long HorseId { get; set; }
        [Key]
        [Column(TypeName = "nvarchar(40)")]
        public long UserId { get; set; }
        [Column(TypeName = "nvarchar(8)")]
        public string RuleAccess { get; set; } = null!;

        [ForeignKey("Id")]
        [InverseProperty("HorseOwners")]
        public virtual User User { get; set; } = null!;
        [ForeignKey("HorseId")]
        [InverseProperty("HorseOwners")]
        public virtual Horse Horse { get; set; } = null!;
    }

    public static class HorseRuleAccess
    {
        public static readonly string Read = "Read";
        public static readonly string Write = "Write";
        public static readonly string Owner = "Owner";
    }
}

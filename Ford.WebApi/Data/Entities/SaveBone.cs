using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ford.WebApi.Data.Entities
{
    public partial class SaveBone
    {
        [Key]
        public long SaveId { get; set; }
        [Column(TypeName = "nvarchar(18)")]
        public string BoneId { get; set; } = null!;
        public float? RotationX { get; set; }
        public float? RotationY { get; set; }
        public float? RotationZ { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        public float? PositionZ { get; set; }

        [ForeignKey("SaveId")]
        [InverseProperty("SaveBones")]
        public virtual Save Save { get; set; } = null!;
    }
}

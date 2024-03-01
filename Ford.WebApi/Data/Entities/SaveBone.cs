using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ford.WebApi.Data.Entities;

public class SaveBone
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SaveBoneId { get; set; }
    public long SaveId { get; set; }
    [Column(TypeName = "nvarchar(32)")]
    public string BoneId { get; set; } = null!;
    public float? RotationX { get; set; }
    public float? RotationY { get; set; }
    public float? RotationZ { get; set; }
    public float? PositionX { get; set; }
    public float? PositionY { get; set; }
    public float? PositionZ { get; set; }

    public Save Save { get; set; } = null!;
}

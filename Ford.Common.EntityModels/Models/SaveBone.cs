using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ford.Models
{
    public partial class SaveBone
    {
        [Key]
        public long SaveId { get; set; }
        [Key]
        [Column(TypeName = "nvarchar(18)")]
        public string BoneId { get; set; } = null!;
        public float? VectorRotationX { get; set; }
        public float? VectorRotationY { get; set; }
        public float? VectorRotationZ { get; set; }
        public float? PositionVectorX { get; set; }
        public float? PositionVectorY { get; set; }
        public float? PositionVectorZ { get; set; }

        [ForeignKey("BoneId")]
        [InverseProperty("SaveBones")]
        public virtual Bone Bone { get; set; } = null!;
        [ForeignKey("SaveId")]
        [InverseProperty("SaveBones")]
        public virtual Save Save { get; set; } = null!;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ford.EntityModels
{
    public partial class Vector
    {
        [Key]
        public long VectorId { get; set; }
        public long? BoneId { get; set; }
        [Column(TypeName = "nvarchar(8)")]
        public string Type { get; set; } = null!;
        [Column(TypeName = "float")]
        public double? X { get; set; }
        [Column(TypeName = "float")]
        public double? Y { get; set; }
        [Column(TypeName = "float")]
        public double? Z { get; set; }

        [ForeignKey("BoneId")]
        [InverseProperty("Vectors")]
        public virtual HorseSafe? Bone { get; set; }
    }
}

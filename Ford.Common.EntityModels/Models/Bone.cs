using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ford.EntityModels
{
    public partial class Bone
    {
        [Key]
        [Column(TypeName = "nvarchar(18)")]
        public string BoneId { get; set; } = null!;
        [Column(TypeName = "nvarchar(18)")]
        public string? GroupId { get; set; }
        public long? HorseSaveId { get; set; }

        [ForeignKey("HorseSaveId")]
        [InverseProperty("Bones")]
        public virtual HorseSafe? HorseSave { get; set; }
    }
}

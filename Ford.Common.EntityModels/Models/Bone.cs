using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ford.Common.EntityModels.Models
{
    public partial class Bone
    {
        public Bone()
        {
            SaveBones = new HashSet<SaveBone>();
        }

        [Key]
        [Column(TypeName = "nvarchar(18)")]
        public string BoneId { get; set; } = null!;
        [Column(TypeName = "nvarchar(18)")]
        public string GroupId { get; set; } = null!;
        [Column(TypeName = "nvarchar(60)")]
        public string? Name { get; set; }

        [InverseProperty("Bone")]
        public virtual ICollection<SaveBone> SaveBones { get; set; }
    }
}

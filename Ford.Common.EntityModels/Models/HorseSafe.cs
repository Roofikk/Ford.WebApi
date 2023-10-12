using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ford.EntityModels
{
    public partial class HorseSafe
    {
        public HorseSafe()
        {
            Bones = new HashSet<Bone>();
            Vectors = new HashSet<Vector>();
        }

        [Key]
        public long HorseSaveId { get; set; }
        public long? HorseId { get; set; }
        [Column(TypeName = "nvarchar(30)")]
        public string? Header { get; set; }
        [Column(TypeName = "datetime")]
        public byte[]? Date { get; set; }

        [ForeignKey("HorseId")]
        [InverseProperty("HorseSaves")]
        public virtual Horse? Horse { get; set; }
        [InverseProperty("HorseSave")]
        public virtual ICollection<Bone> Bones { get; set; }
        [InverseProperty("Bone")]
        public virtual ICollection<Vector> Vectors { get; set; }
    }
}

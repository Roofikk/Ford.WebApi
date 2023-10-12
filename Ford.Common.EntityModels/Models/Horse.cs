using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ford.EntityModels
{
    public partial class Horse
    {
        public Horse()
        {
            HorseSaves = new HashSet<HorseSafe>();
            Users = new HashSet<User>();
        }

        [Key]
        public long HorseId { get; set; }
        [Column(TypeName = "ncarchar(15)")]
        public string? HorseName { get; set; }
        [Column(TypeName = "datetime")]
        public byte[]? BirthDate { get; set; }
        [Column(TypeName = "nvarchar(8)")]
        public string Sex { get; set; } = null!;
        [Column(TypeName = "nvarchar(15)")]
        public string? City { get; set; }
        [Column(TypeName = "nvarchar(15)")]
        public string? Region { get; set; }
        [Column(TypeName = "nvarchar(15)")]
        public string? Country { get; set; }

        [InverseProperty("Horse")]
        public virtual ICollection<HorseSafe> HorseSaves { get; set; }

        [ForeignKey("HorseId")]
        [InverseProperty("Horses")]
        public virtual ICollection<User> Users { get; set; }
    }
}

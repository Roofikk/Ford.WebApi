using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ford.EntityModels.Models
{
    public partial class Horse
    {
        public Horse()
        {
            Saves = new HashSet<Save>();
            HorseOwners = new HashSet<HorseOwner>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long HorseId { get; set; }
        [Column(TypeName = "ncarchar(30)")]
        public string Name { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? BirthDate { get; set; }
        [Column(TypeName = "nvarchar(6)")]
        public Sex Sex { get; set; }
        [Column(TypeName = "nvarchar(15)")]
        public string? City { get; set; }
        [Column(TypeName = "nvarchar(15)")]
        public string? Region { get; set; }
        [Column(TypeName = "nvarchar(15)")]
        public string? Country { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime CreationDate { get; set; }

        [InverseProperty("Horse")]
        public virtual ICollection<Save> Saves { get; set; }

        [InverseProperty("Horse")]
        public virtual ICollection<HorseOwner> HorseOwners { get; set; }
    }

    public enum Sex
    {
        None,
        Male,
        Female
    }
}

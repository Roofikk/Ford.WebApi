using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ford.EntityModels
{
    public partial class User
    {
        public User()
        {
            Horses = new HashSet<Horse>();
        }

        [Key]
        public long UserId { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public string Login { get; set; } = null!;
        [Column(TypeName = "nvarchar(32)")]
        public string Password { get; set; } = null!;
        [Column(TypeName = "nvarchar(128)")]
        public string? Email { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public string? Name { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public string? LastName { get; set; }
        [Column(TypeName = "nvarchar(24)")]
        public string? Phone { get; set; }
        [Column(TypeName = "nvarchar(25)")]
        public string? City { get; set; }
        [Column(TypeName = "nvarchar(25)")]
        public string? Region { get; set; }
        [Column(TypeName = "nvarchar(25)")]
        public string? Country { get; set; }
        [Column(TypeName = "datetime")]
        public byte[]? BirthDate { get; set; }
        [Column(TypeName = "datetime")]
        public byte[]? RegistrationDate { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("Users")]
        public virtual ICollection<Horse> Horses { get; set; }
    }
}

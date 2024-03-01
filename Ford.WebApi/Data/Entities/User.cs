using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;

namespace Ford.WebApi.Data.Entities;

public class User : IdentityUser<long>
{
    public User() : base()
    {
        HorseOwners = [];
        Saves = [];
    }

    [Column(TypeName = "varchar(64)")]
    public override string? UserName { get => base.UserName; set => base.UserName = value; }
    [Column(TypeName = "varchar(32)")]
    public override string? PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }
    [Column(TypeName = "varchar(64)")]
    public override string? Email { get => base.Email; set => base.Email = value; }

    [Column(TypeName = "varchar(20)")]
    public string FirstName { get; set; } = null!;
    [Column(TypeName = "varchar(20)")]
    public string? LastName { get; set; }
    [Column(TypeName = "varchar(25)")]
    public string? City { get; set; }
    [Column(TypeName = "varchar(25)")]
    public string? Region { get; set; }
    [Column(TypeName = "varchar(25)")]
    public string? Country { get; set; }
    [Column(TypeName = "date")]
    public DateTime? BirthDate { get; set; }
    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }
    [Column(TypeName = "datetime")]
    public DateTime LastUpdatedDate { get; set; }
    [Column(TypeName = "varchar(64)")]
    public string RefreshToken { get; set; } = string.Empty;
    [Column(TypeName = "datetime")]
    public DateTime RefreshTokenExpiresDate { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<HorseOwner> HorseOwners { get; }
    public virtual ICollection<Save> Saves { get; }
}

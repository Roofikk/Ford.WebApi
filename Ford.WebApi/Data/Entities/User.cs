using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    [RegularExpression(@"^([\+]?(?:00)?[0-9]{1,3}[\s.-]?[0-9]{1,12})([\s.-]?[0-9]{1,4}?)$")]
    [Column(TypeName = "varchar(32)")]
    public override string? PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }
    [Column(TypeName = "varchar(64)")]
    public override string? Email { get => base.Email; set => base.Email = value; }

    [Column(TypeName = "varchar(64)")]
    public string FirstName { get; set; } = null!;
    [Column(TypeName = "varchar(64)")]
    public string? LastName { get; set; }
    [Column(TypeName = "varchar(64)")]
    public string? City { get; set; }
    [Column(TypeName = "varchar(64)")]
    public string? Region { get; set; }
    [Column(TypeName = "varchar(64)")]
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
    public virtual ICollection<UserHorse> HorseOwners { get; }
    public virtual ICollection<Save> Saves { get; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Ford.WebApi.Data.Entities;

public class User : IdentityUser<long>
{
    [Column(TypeName = "varchar(64)")]
    public override string? UserName { get => base.UserName; set => base.UserName = value; }
    [Column(TypeName = "varchar(64)")]
    public override string? NormalizedUserName { get => base.NormalizedUserName; set => base.NormalizedUserName = value; }
    [Column(TypeName = "varchar(32)")]
    [RegularExpression(@"^([\+]?[0-9]{0,3}[-\s.]?[(\s]?[0-9]{3}[)\s]?[-\s.]?([0-9]{2,3}[-\s]?){2}[0-9]{2})$")]
    public override string? PhoneNumber
    { get => base.PhoneNumber; set => base.PhoneNumber = value; }
    [Column(TypeName = "varchar(64)")]
    public override string? Email { get => base.Email; set => base.Email = value; }
    [Column(TypeName = "varchar(64)")]
    public override string? NormalizedEmail { get => base.NormalizedEmail; set => base.NormalizedEmail = value; }
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
    public DateOnly? BirthDate { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastUpdate { get; set; }
    [Column(TypeName = "varchar(128)")]
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresDate { get; set; }

    [InverseProperty("User")]
    public ICollection<HorseUser> HorseUsers { get; } = [];
    public ICollection<Horse> Horses { get; } = [];
}

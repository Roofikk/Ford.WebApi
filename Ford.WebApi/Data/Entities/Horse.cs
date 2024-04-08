using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ford.WebApi.Data.Entities;

public class Horse
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long HorseId { get; set; }
    public long? LastModifiedByUserId { get; set; }
    [Column(TypeName = "varchar(64)")]
    public string Name { get; set; } = null!;
    [Column(TypeName = "TEXT")]
    public string? Description { get; set; }
    public DateOnly? BirthDate { get; set; }
    [Column(TypeName = "varchar(16)")]
    public string? Sex { get; set; }
    [Column(TypeName = "nvarchar(32)")]
    public string? City { get; set; }
    [Column(TypeName = "nvarchar(64)")]
    public string? Region { get; set; }
    [Column(TypeName = "nvarchar(32)")]
    public string? Country { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastModified { get; set; }
    [Column(TypeName = "varchar(32)")]
    public string? OwnerName { get; set; }
    [Column(TypeName = "varchar(32)")]
    public string? OwnerPhoneNumber { get; set; }

    public virtual User? LastModifiedByUser { get; set; }
    public virtual ICollection<Save> Saves { get; } = [];
    public virtual ICollection<HorseUser> HorseUsers { get; } = [];
    public virtual ICollection<User> Users { get; } = [];
}

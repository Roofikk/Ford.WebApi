using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ford.WebApi.Data.Entities;

public class Horse
{
    public Horse()
    {
        Saves = new HashSet<Save>();
        HorseOwners = new HashSet<HorseOwner>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long HorseId { get; set; }
    [Column(TypeName = "varchar(64)")]
    public string Name { get; set; } = null!;
    [Column(TypeName = "TEXT")]
    public string? Description { get; set; }
    [Column(TypeName = "date")]
    public DateTime? BirthDate { get; set; }
    [Column(TypeName = "varchar(16)")]
    public string? Sex { get; set; }
    [Column(TypeName = "nvarchar(32)")]
    public string? City { get; set; }
    [Column(TypeName = "nvarchar(64)")]
    public string? Region { get; set; }
    [Column(TypeName = "nvarchar(32)")]
    public string? Country { get; set; }
    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }
    [Column(TypeName = "datetime")]
    public DateTime LastUpdate { get; set; }

    public virtual ICollection<Save> Saves { get; }

    public virtual ICollection<HorseOwner> HorseOwners { get; }
}

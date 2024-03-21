using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ford.WebApi.Data.Entities;

public class Save
{
    public Save()
    {
        SaveBones = new HashSet<SaveBone>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SaveId { get; set; }
    public long HorseId { get; set; }
    public long? CreatedByUserId { get; set; }
    public long? LastUpdatedByUserId { get; set; }
    [Column(TypeName = "varchar(64)")]
    public string Header { get; set; } = null!;
    [Column(TypeName = "TEXT")]
    public string? Description { get; set; }
    [Column(TypeName = "datetime")]
    public DateTime Date { get; set; }
    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }
    [Column(TypeName = "datetime")]
    public DateTime LastUpdate { get; set; }

    public Horse Horse { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public User? LastUpdatedByUser { get; set; }
    public virtual ICollection<SaveBone> SaveBones { get; }
}

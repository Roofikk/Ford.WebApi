using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ford.WebApi.Data.Entities;

public class Save
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SaveId { get; set; }
    public long HorseId { get; set; }
    public long? CreatedByUserId { get; set; }
    public long? LastModifiedByUserId { get; set; }
    [Column(TypeName = "varchar(64)")]
    public string Header { get; set; } = null!;
    [Column(TypeName = "TEXT")]
    public string? Description { get; set; }
    public DateOnly Date { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastModified { get; set; }

    public Horse Horse { get; set; } = null!;
    public virtual User? CreatedByUser { get; set; }
    public virtual User? LastUpdatedByUser { get; set; }
    public virtual ICollection<SaveBone> SaveBones { get; } = [];
}

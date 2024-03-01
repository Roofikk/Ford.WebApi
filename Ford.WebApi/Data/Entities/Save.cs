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
    public long UserId { get; set; }
    [Column(TypeName = "varchar(64)")]
    public string Header { get; set; } = null!;
    [Column(TypeName = "TEXT")]
    public string? Description { get; set; }
    [Column(TypeName = "datetime")]
    public DateTime? Date { get; set; }

    public virtual Horse Horse { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ICollection<SaveBone> SaveBones { get; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ford.WebApi.Data.Entities;

public class HorseUser
{
    public long HorseId { get; set; }
    public long UserId { get; set; }
    // переименовать в AccessRole
    [Column(TypeName = "nvarchar(8)")]
    public string AccessRole { get; set; } = null!;
    public bool IsOwner { get; set; } = false;
    public virtual User User { get; set; } = null!;
    public virtual Horse Horse { get; set; } = null!;
}

public enum UserAccessRole
{
    // Only read saves and horse info
    Viewer = 0,
    // Read/Write horse info and add/edit saves
    Writer = 1,
    // Also add/remove other users to horse
    All = 2,
    // There can only be one creator
    Creator = 3
}

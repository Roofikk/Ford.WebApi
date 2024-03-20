using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ford.WebApi.Data.Entities;

public class UserHorse
{
    [Key]
    public long HorseId { get; set; }
    [Key]
    public long UserId { get; set; }
    // переименовать в AccessRole
    [Column(TypeName = "nvarchar(8)")]
    public string RuleAccess { get; set; } = null!;
    public bool IsOwner { get; set; } = false;
    public User User { get; set; } = null!;
    public Horse Horse { get; set; } = null!;
}

public enum UserAccessRole
{
    // Only read saves and horse info
    Read = 0,
    // Read/Write horse info and add/edit saves
    Write = 1,
    // Also add/remove other users to horse
    All = 2,
    // There can only be one creator
    Creator = 3
}

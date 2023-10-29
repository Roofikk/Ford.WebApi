using Ford.WebApi.Models;

namespace Ford.WebApi.Dtos.Save;

public class BoneDto
{
    public string BoneId { get; set; } = null!;
    public string GroupId { get; set; } = null!;
    public string? Name { get; set; }
    public Vector? Rotation { get; set; }
    public Vector? Position { get; set; }
}

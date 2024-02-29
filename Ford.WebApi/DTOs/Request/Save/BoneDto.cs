using Ford.WebApi.Models;

namespace Ford.WebApi.Dtos.Request;

public class BoneDto
{
    public string BoneId { get; set; } = null!;
    public Vector? Rotation { get; set; }
    public Vector? Position { get; set; }
}

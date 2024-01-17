using Ford.WebApi.Data.Entities;

namespace Ford.WebApi.Dtos.Save;

public class ResponseSaveDto
{
    public long SaveId { get; set; }
    public long HorseId { get; set; }
    public string Header { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
    public ICollection<BoneDto> Bones { get; set; } = null!;
}

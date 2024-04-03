using Ford.WebApi.Dtos.Horse;

namespace Ford.WebApi.Dtos.Request;

public class SaveCreatingDto : IRequestSave
{
    public long HorseId { get; set; }
    public string Header { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly Date { get; set; }
    public ICollection<BoneDto> Bones { get; set; } = [];
}

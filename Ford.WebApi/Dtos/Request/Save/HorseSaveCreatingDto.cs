namespace Ford.WebApi.Dtos.Request;

public class HorseSaveCreatingDto : IRequestSave
{
    public string Header { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly Date { get; set; }
    public ICollection<BoneDto> Bones { get; set; } = [];
}

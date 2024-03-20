namespace Ford.WebApi.Dtos.Request;

public class RequestCreateSaveDto : IRequestSave
{
    public long HorseId { get; set; }
    public string Header { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public ICollection<BoneDto> Bones { get; set; } = null!;
}

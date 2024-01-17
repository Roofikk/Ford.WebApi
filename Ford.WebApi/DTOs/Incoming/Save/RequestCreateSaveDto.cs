namespace Ford.WebApi.Dtos.Save;

public class RequestCreateSaveDto
{
    public string Header { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public DateTime? Date { get; set; }
    public ICollection<BoneDto> Bones { get; set; } = null!;
}

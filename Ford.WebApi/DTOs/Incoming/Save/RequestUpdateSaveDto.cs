namespace Ford.WebApi.Dtos.Save;

public class RequestUpdateSaveDto
{
    public long HorseId { get; set; }
    public string Header { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
}
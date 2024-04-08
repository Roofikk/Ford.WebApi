using Ford.WebApi.Dtos.Horse;

namespace Ford.WebApi.Dtos.Response;

public class SaveDto : IResponseSave
{
    public long SaveId { get; set; }
    public long HorseId { get; set; }
    public string Header { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly Date { get; set; }
    public UserDateDto CreatedBy { get; set; } = null!;
    public UserDateDto LastModifiedBy { get; set; } = null!;
}

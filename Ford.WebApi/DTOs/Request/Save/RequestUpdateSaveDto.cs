using Ford.WebApi.Dtos.Horse;

namespace Ford.WebApi.Dtos.Request;

public class RequestUpdateSaveDto : IRequestSave, IStorageAction
{
    public long SaveId { get; set; }
    public string Header { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public DateOnly Date { get; set; }
}

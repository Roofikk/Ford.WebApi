using Ford.WebApi.Dtos.Horse;

namespace Ford.WebApi.Dtos.Response;

public interface IResponseSave
{
    public long SaveId { get; set; }
    public long HorseId { get; set; }
    public string Header { get; set; }
    public string? Description { get; set; }
    public DateOnly? Date { get; set; }
    public HorseUserDto CreatedByUser { get; set; }
    public HorseUserDto LastUpdatedUser { get; set; }
}

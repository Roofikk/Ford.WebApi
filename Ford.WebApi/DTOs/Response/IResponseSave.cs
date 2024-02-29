namespace Ford.WebApi.Dtos.Response;

public interface IResponseSave
{
    public long SaveId { get; set; }
    public long HorseId { get; set; }
    public string Header { get; set; }
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
}

namespace Ford.WebApi.Dtos.Request;

public interface IRequestSave
{
    public string Header { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
}

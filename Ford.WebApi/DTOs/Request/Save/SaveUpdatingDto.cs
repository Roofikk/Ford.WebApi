﻿namespace Ford.WebApi.Dtos.Request;

public class SaveUpdatingDto : IRequestSave
{
    public long SaveId { get; set; }
    public string Header { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public DateOnly Date { get; set; }
    public DateTime? LastModified { get; set; }
}

﻿namespace Ford.WebApi.Dtos.Request;

public class RequestUpdateSaveDto
{
    public string Header { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public DateTime? Date { get; set; }
}

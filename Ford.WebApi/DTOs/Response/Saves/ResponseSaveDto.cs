﻿using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Dtos.Request;

namespace Ford.WebApi.Dtos.Response;

public class ResponseSaveDto : IResponseSave
{
    public long SaveId { get; set; }
    public long HorseId { get; set; }
    public string Header { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public HorseUserDto CreatedByUser { get; set; } = null!;
    public HorseUserDto LastUpdatedUser { get; set; } = null!;
}

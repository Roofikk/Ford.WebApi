using Ford.WebApi.Dtos.Horse;

namespace Ford.WebApi.Dtos.Request;

public class SaveCreatingDto : HorseSaveCreatingDto
{
    public long HorseId { get; set; }
}

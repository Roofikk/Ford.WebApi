using Ford.WebApi.Dtos.Horse;

namespace Ford.WebApi.Dtos.Response;

public class HistoryDto
{
    public ICollection<ServiceResult<StorageHistory>> Successful { get; } = [];
    public ICollection<StorageHistory> Failures { get; } = [];
}

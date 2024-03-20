using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Request;
using Ford.WebApi.Dtos.Response;

namespace Ford.WebApi.Services;

public interface ISaveRepository
{
    public Task<ICollection<ResponseSaveDto>> GetAsync(long horseId, long userId, int below = 0, int amount = 20);
    public Task<ResponseFullSave?> GetAsync(long horseId, long saveId, long userId);
    public ResponseResult<Horse> Create(Horse horse, ICollection<RequestCreateSaveDto> requestCreateSaves, long userId);
    public ResponseResult<Horse> Create(Horse horse, RequestCreateSaveDto requestSave, long userId);
    public Task<ResponseSaveDto?> CreateAsync(RequestCreateSaveDto requestSave, long userId);
    public Task<ResponseSaveDto?> UpdateAsync(RequestUpdateSaveDto requestSave, Save save);
    public Task<bool> DeleteAsync(Save save);
}

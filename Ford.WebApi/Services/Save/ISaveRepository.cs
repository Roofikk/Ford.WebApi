using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Request;
using Ford.WebApi.Dtos.Response;

namespace Ford.WebApi.Services;

public interface ISaveRepository
{
    public Task<FullSaveDto?> GetAsync(long horseId, long saveId, long userId);
    public Task<ICollection<SaveDto>> GetAsync(long horseId, long userId, int below = 0, int amount = 20);
    public ServiceResult<ICollection<Save>> Create(ICollection<SaveCreatingDto> requestCreateSaves, long userId);
    public ServiceResult<Save> Create(SaveCreatingDto requestSave, long userId);
    public Task<ServiceResult<SaveDto>> CreateAsync(SaveCreatingDto requestSave, long userId);
    public Task<ServiceResult<SaveDto>> UpdateAsync(RequestUpdateSaveDto requestSave, long userId);
    public Task<bool> DeleteAsync(long saveId);
    public Task<int> SaveChangesAsync();
}

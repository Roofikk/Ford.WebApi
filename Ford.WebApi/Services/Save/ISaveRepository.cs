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
    public Task<SaveDto?> CreateAsync(SaveCreatingDto requestSave, long userId);
    public Task<SaveDto?> UpdateAsync(RequestUpdateSaveDto requestSave, Save save, long userId);
    public Task<bool> DeleteAsync(Save save);
    public Task<int> SaveChangesAsync();
}

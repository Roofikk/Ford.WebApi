using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Request;
using Ford.WebApi.Dtos.Response;

namespace Ford.WebApi.Services;

public interface ISaveRepository
{
    public Task<FullSaveDto?> GetAsync(long horseId, long saveId, long userId);
    public Task<ICollection<SaveDto>> GetAsync(long horseId, long userId, int below = 0, int amount = 20);
    public ServiceResult<ICollection<Save>> Create(ICollection<HorseSaveCreatingDto> requestCreateSaves, Horse horse, long userId);
    public Task<ServiceResult<ICollection<Save>>> CreateSavesToExistHorseAsync(ICollection<SaveCreatingDto> requestCreateSaves, long userId);
    public Task<ServiceResult<Save>> CreateToExistHorseAsync(SaveCreatingDto requestSave, long userId);
    public Task<ServiceResult<SaveDto>> UpdateAsync(SaveUpdatingDto requestSave, long userId);
    public Task<bool> DeleteAsync(long saveId, long userId);
    public Task<int> SaveChangesAsync();
}

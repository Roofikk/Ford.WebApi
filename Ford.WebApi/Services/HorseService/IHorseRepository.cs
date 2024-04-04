using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Dtos.Response;

namespace Ford.WebApi.Services.HorseService;

public interface IHorseRepository
{
    public Task<HorseDto?> GetByIdAsync(long userId, long horseId);
    public Task<ICollection<HorseDto>> GetAsync(long userId, int below = 0, int amount = 20,
        string orderByDate = "desc", string orderByName = "false");
    public Task<ServiceResult<HorseDto>> CreateAsync(User user, HorseCreatingDto horseDto);
    public Task<ServiceResult<HorseDto>> UpdateAsync(User user, HorseUpdatingDto horseDto);
    public Task<bool> DeleteAsync(long horseId, long userId);
    public Task<int> SaveChangesAsync();
}

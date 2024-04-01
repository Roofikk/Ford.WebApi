using Ford.WebApi.Dtos.Horse;

namespace Ford.WebApi.Services.HorseService;

public interface IHorseRepository
{
    public Task<HorseRetrievingDto?> GetByIdAsync(long horseId, long userId);
    public Task<ICollection<HorseRetrievingDto>> GetAsync(long userId, int below = 0, int amount = 20,
        string orderByDate = "desc", string orderByName = "false");
    public Task<HorseRetrievingDto> CreateAsync(HorseCreatingDto horseDto);
    public Task<HorseRetrievingDto?> UpdateAsync(HorseUpdatingDto horseDto);
    public Task<bool> DeleteAsync(long horseId);
    public Task SaveChangesAsync();
}

using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Dtos.Response;
using Ford.WebApi.Models.Horse;

namespace Ford.WebApi.Services;

public interface IUserHorseRepository
{
    public Task<ICollection<HorseUserDto>> GetAsync(long horseId);
    public ServiceResult<ICollection<UserHorse>> Create(User user, long horseId, ICollection<RequestHorseUser> requestHorseUser);
    public ServiceResult<ICollection<UserHorse>> Create(User user, Horse horse, ICollection<RequestHorseUser> requestHorseUser);
    public Task<ServiceResult<ICollection<UserHorse>>> UpdateAsync(long currentUserId, long horseId, ICollection<RequestHorseUser> resuestHorseUsers);
    public Task<ServiceResult<ICollection<UserHorse>>> AddAsync(long currentUserId, long horseId, RequestHorseUser horseUser);
    public Task<ServiceResult> DeleteAsync(long currentUserId, long horseId, long userId);
    public Task<int> SaveChangesAsync();
}

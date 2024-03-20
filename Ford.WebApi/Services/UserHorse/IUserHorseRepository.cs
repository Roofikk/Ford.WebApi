using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Dtos.Response;
using Ford.WebApi.Models.Horse;

namespace Ford.WebApi.Services;

public interface IUserHorseRepository
{
    public Task<ICollection<HorseUserDto>> GetAsync(long horseId);
    public ResponseResult<Horse> Create(User user, Horse horse, ICollection<RequestHorseUser> requestHorseUser);
    public Task<ResponseResult<ICollection<HorseUserDto>>> UpdateAsync(long currentUserId, long horseId, ICollection<RequestHorseUser> resuestHorseUsers);
    public Task<ResponseResult<Horse>> UpdateAsync(long currentUserId, Horse horse, ICollection<RequestHorseUser> resuestHorseUsers);
    public Task<ResponseResult<ICollection<HorseUserDto>>> AddAsync(long currentUserId, long horseId, RequestHorseUser horseUser);
    public Task<ResponseResult> DeleteAsync(long currentUserId, long horseId, long userId);
}

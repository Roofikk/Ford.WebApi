using Ford.Models;

namespace Ford.WebApi.Repositories;

public interface IUserRepository
{
    Task<User?> CreateAsync(User user);
    Task<IEnumerable<User>> RetrieveAllAsync();
    Task<User?> RetrieveAsync(string id);
    Task<User?> UpdateAsync(User user);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsExist(string id, string login);
    Task<bool> IsExist(string id);
    Task<bool> Save();
}

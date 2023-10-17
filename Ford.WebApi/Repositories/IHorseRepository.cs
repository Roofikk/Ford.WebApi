using Ford.Models;

namespace Ford.WebApi.Repositories
{
    public interface IHorseRepository : IRepository<Horse, long>
    {
        //Task<Horse> AddUsers(IEnumerable<User> users);
    }
}

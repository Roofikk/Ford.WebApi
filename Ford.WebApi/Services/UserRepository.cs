using Microsoft.EntityFrameworkCore;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Data;

namespace Ford.WebApi.Repositories;

public class UserRepository : IRepository<User, long>
{
    private FordContext db;

    public UserRepository(FordContext db)
    {
        this.db = db;
    }

    public async Task<User?> CreateAsync(User user)
    {
        var added = await db.Users.AddAsync(user);
        return added.Entity;
    }

    public async Task<IEnumerable<User>?> RetrieveAllAsync()
    {
        IEnumerable<User>? users = await db.Users.ToListAsync();
        return users;
    }

    public async Task<User?> RetrieveAsync(long id)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> UpdateAsync(User user)
    {
        User? find = await RetrieveAsync(user.Id);

        if (find is null)
        {
            return null;
        }

        user.Email = find.Email;
        user.CreationDate = find.CreationDate;
        db.Entry(find).CurrentValues.SetValues(user);
        return user;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        User? user = await RetrieveAsync(id);

        if (user is not null)
        {
            db.Users.Remove(user);
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> IsExistAsync(User user)
    {
        return await IsExistAsync(user.Id);
    }

    public async Task<bool> IsExistAsync(long id)
    {
        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        return user is not null;
    }

    public async Task<bool> SaveAsync()
    {
        return (await db.SaveChangesAsync()) == 1;
    }
}

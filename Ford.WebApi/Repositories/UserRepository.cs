using Ford.DataContext.Sqlite;
using Ford.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ford.WebApi.Repositories;

public class UserRepository : IRepository<User, string>
{
    private FordContext db;

    public UserRepository(FordContext db)
    {
        this.db = db;
    }

    public async Task<User?> CreateAsync(User user)
    {
        EntityEntry<User> added = await db.Users.AddAsync(user);
        return added.Entity;
    }

    public async Task<IEnumerable<User>?> RetrieveAllAsync()
    {
        IEnumerable<User>? users = await db.Users.ToListAsync();
        return users;
    }

    public async Task<User?> RetrieveAsync(string id)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User?> UpdateAsync(User user)
    {
        User? find = await RetrieveAsync(user.UserId);

        if (find is null)
        {
            return null;
        }

        user.Login = find.Login;
        user.CreationDate = find.CreationDate;
        db.Entry(find).CurrentValues.SetValues(user);
        return user;
    }

    public async Task<bool> DeleteAsync(string id)
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
        return await IsExistAsync(user.UserId);
    }

    public async Task<bool> IsExistAsync(string id)
    {
        User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id);
        return user is not null;
    }

    public async Task<bool> SaveAsync()
    {
        return (await db.SaveChangesAsync()) == 1;
    }
}

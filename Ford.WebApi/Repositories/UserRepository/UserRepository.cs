using Ford.DataContext.Sqlite;
using Ford.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ford.WebApi.Repositories;

public class UserRepository : IUserRepository
{
    private FordContext db;

    public UserRepository(FordContext db)
    {
        this.db = db;
    }

    public async Task<User?> CreateAsync(User user)
    {
        user.UserId = Guid.NewGuid().ToString();
        EntityEntry<User> added = await db.Users.AddAsync(user);
        return added.Entity;
    }

    public Task<IEnumerable<User>> RetrieveAllAsync()
    {
        return Task.FromResult<IEnumerable<User>>(db.Users);
    }

    public async Task<User?> RetrieveAsync(string id)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User?> UpdateAsync(User user)
    {
        User? find = await db.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);
        db.Entry(find).State = EntityState.Detached;
        EntityEntry<User> updated = db.Users.Update(user);
        return updated.Entity;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id);

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

    public async Task<bool> IsExist(string userId, string userLogin)
    {
        User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId || u.Login == userLogin);
        return user is not null;
    }

    public async Task<bool> IsExist(string id)
    {
        User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id);
        return user is not null;
    }

    public async Task<bool> Save()
    {
        return (await db.SaveChangesAsync()) == 1;
    }
}

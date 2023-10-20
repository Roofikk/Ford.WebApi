using Ford.Common.EntityModels.Models;
using Ford.DataContext.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ford.WebApi.Repositories;

public class HorseRepository : IHorseRepository
{
    private readonly FordContext db;

    public HorseRepository(FordContext db)
    {
        this.db = db;
    }

    public async Task<IEnumerable<Horse>?> RetrieveAllAsync()
    {
        IEnumerable<Horse>? horses = await db.Horses.ToListAsync();
        return horses;
    }

    public async Task<Horse?> RetrieveAsync(long id)
    {
        Horse? horse = await db.Horses.FirstOrDefaultAsync(h => h.HorseId == id);
        return horse;
    }

    public async Task<Horse?> CreateAsync(Horse entity)
    {
        IEnumerable<User> find =  db.Users.Intersect(entity.Users);

        if (find.Count() != entity.Users.Count)
        {
            return null;
        }

        await db.Horses.AddAsync(entity);
        return entity;
    }

    public async Task<Horse?> UpdateAsync(Horse entity)
    {
        Horse? find = await RetrieveAsync(entity.HorseId);

        if (find is null)
        {
            return null;
        }

        entity.CreationDate = find.CreationDate;
        entity.Saves = find.Saves;
        db.Entry(find).CurrentValues.SetValues(entity);
        db.Entry(find).State = EntityState.Modified;
        return entity;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        Horse? horse = await RetrieveAsync(id);

        if (horse is null) { return false; }
        db.Horses.Remove(horse);
        return true;
    }

    public async Task<bool> IsExistAsync(long id)
    {
        return await db.Horses.FirstOrDefaultAsync(h => h.HorseId == id) is not null;
    }

    public async Task<bool> IsExistAsync(Horse entity)
    {
        return await IsExistAsync(entity.HorseId);
    }

    public async Task<bool> SaveAsync()
    {
        return (await db.SaveChangesAsync()) == 1;
    }

    //public async Task<bool> AddUser(string userId, Horse horse)
    //{
    //    User? user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

    //    if (user is null)
    //    {
    //        return false;
    //    }

    //    Horse entryHorse = horse;
    //    entryHorse.Users.Add(user);
    //    db.Entry(horse).CurrentValues.SetValues(entryHorse);
    //}
}

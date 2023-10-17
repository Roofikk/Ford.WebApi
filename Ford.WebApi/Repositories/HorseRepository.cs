using Ford.DataContext.Sqlite;
using Ford.Models;
using Microsoft.EntityFrameworkCore;

namespace Ford.WebApi.Repositories;

public class HorseRepository : IRepository<Horse, long>
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
        db.Entry(find).CurrentValues.SetValues(entity);
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
}

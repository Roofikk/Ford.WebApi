using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Horse;
using System.Data.Entity;

namespace Ford.WebApi.Services.HorseService;

public class HorseRepository : IHorseRepository
{
    private readonly FordContext _context;
    private readonly ISaveRepository _saveService;

    public HorseRepository(FordContext context, ISaveRepository saveService)
    {
        _context = context;
        _saveService = saveService;
    }

    public async Task<HorseRetrievingDto?> GetByIdAsync(long horseId, long userId)
    {
        var horse = await _context.Horses.SingleOrDefaultAsync(h => h.HorseId == horseId && h.Users.Any(u => u.UserId == userId));

        if (horse == null)
        {
            return null;
        }

        return await MapHorse(horse, userId);
    }

    public async Task<ICollection<HorseRetrievingDto>> GetAsync(long userId, int below = 0,
        int amount = 20, string orderByDate = "desc", string orderByName = "false")
    {
        var queryableHorses = _context.Horses.Where(h => h.Users.Any(o => o.UserId == userId));

        switch (orderByDate)
        {
            case "true":
                queryableHorses = queryableHorses.OrderBy(o => o.LastUpdate);
                break;
            case "desc":
                queryableHorses = queryableHorses.OrderByDescending(o => o.LastUpdate);
                break;
        }

        switch (orderByName)
        {
            case "true":
                queryableHorses = queryableHorses.OrderBy(o => o.Name);
                break;
            case "desc":
                queryableHorses = queryableHorses.OrderByDescending(o => o.Name);
                break;
        }

        IEnumerable<Horse> horses = queryableHorses
            .Skip(below)
            .Take(amount)
            .AsEnumerable();

        List<HorseRetrievingDto> horsesDto = [];
        foreach (var horse in horses)
        {
            var horseDto = await MapHorse(horse, userId);
            horsesDto.Add(horseDto);
        }

        return horsesDto;
    }

    public Task<HorseRetrievingDto> CreateAsync(HorseCreatingDto horseDto)
    {
        throw new NotImplementedException();
    }

    public Task<HorseRetrievingDto> UpdateAsync(HorseUpdatingDto horseDto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(long horseId)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync()
    {
        throw new NotImplementedException();
    }

    private async Task<HorseRetrievingDto> MapHorse(Horse horse, long userId)
    {
        HorseRetrievingDto horseDto = new()
        {
            HorseId = horse.HorseId,
            Name = horse.Name,
            Description = horse.Description,
            BirthDate = horse.BirthDate,
            Sex = horse.Sex,
            City = horse.City,
            Region = horse.Region,
            Country = horse.Country,
            OwnerName = horse.OwnerName,
            OwnerPhoneNumber = horse.OwnerPhoneNumber,
            CreationDate = horse.CreationDate,
            LastUpdate = horse.LastUpdate,
        };

        await _context.Entry(horse).Collection(h => h.Users).LoadAsync();

        foreach (var user in horse.Users)
        {
            await _context.Entry(user).Reference(o => o.User).LoadAsync();

            if (userId == user.UserId)
            {
                horseDto.Self = new()
                {
                    UserId = user.UserId,
                    FirstName = user.User.FirstName,
                    LastName = user.User.LastName,
                    PhoneNumber = user.User.PhoneNumber,
                    AccessRole = user.AccessRole,
                    IsOwner = user.IsOwner,
                };
                continue;
            }

            horseDto.Users.Add(new()
            {
                UserId = user.UserId,
                FirstName = user.User.FirstName,
                LastName = user.User.LastName,
                PhoneNumber = user.User.PhoneNumber,
                IsOwner = user.IsOwner,
                AccessRole = user.AccessRole
            });
        }

        var saves = await _saveService.GetAsync(horse.HorseId, userId, 0, 20);
        horseDto.Saves = saves;

        return horseDto;
    }
}

using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Dtos.Response;
using Microsoft.EntityFrameworkCore;

namespace Ford.WebApi.Services.HorseService;

public class HorseRepository : IHorseRepository
{
    private readonly FordContext _context;
    private readonly ISaveRepository _saveService;
    private readonly IUserHorseRepository _userHorseRepository;

    public HorseRepository(FordContext context, ISaveRepository saveService, 
        IUserHorseRepository userHorseRepository)
    {
        _context = context;
        _saveService = saveService;
        _userHorseRepository = userHorseRepository;
    }

    public async Task<HorseDto?> GetByIdAsync(long userId, long horseId)
    {
        var horse = await _context.Horses.SingleOrDefaultAsync(h => h.HorseId == horseId && h.HorseUsers.Any(u => u.UserId == userId));

        if (horse == null)
        {
            return null;
        }

        return await MapHorse(horse, userId);
    }

    public async Task<ICollection<HorseDto>> GetAsync(long userId, int below = 0,
        int amount = 20, string orderByDate = "desc", string orderByName = "false")
    {
        var queryableHorses = _context.Horses.Where(h => h.HorseUsers.Any(o => o.UserId == userId));

        switch (orderByDate)
        {
            case "true":
                queryableHorses = queryableHorses.OrderBy(o => o.CreationDate);
                break;
            case "desc":
                queryableHorses = queryableHorses.OrderByDescending(o => o.LastModified);
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

        List<HorseDto> horsesDto = [];
        foreach (var horse in horses)
        {
            var horseDto = await MapHorse(horse, userId);
            horsesDto.Add(horseDto);
        }

        return horsesDto;
    }

    public async Task<ServiceResult<HorseDto>> CreateAsync(User user, HorseCreatingDto horseDto)
    {
        Horse horse = new()
        {
            Name = horseDto.Name,
            Description = horseDto.Description,
            BirthDate = horseDto.BirthDate,
            Sex = horseDto.Sex,
            City = horseDto.City,
            Region = horseDto.Region,
            Country = horseDto.Country,
            LastModifiedByUserId = user.Id,
            CreationDate = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        //Check the possibility of granting role to an object
        var check = horseDto.Users
            .Where(u => u.UserId != user.Id && Enum.Parse<UserAccessRole>(u.AccessRole) >= UserAccessRole.Creator);

        if (check.Any())
        {
            return new ServiceResult<HorseDto>()
            {
                Success = false,
                ErrorMessage = "Some roles access can not be greater or equal than your",
            };
        }

        var result = _userHorseRepository.Create(user, horse, horseDto.Users);

        if (!result.Success)
        {
            return new ServiceResult<HorseDto>()
            {
                Success = false,
                ErrorMessage = result.ErrorMessage
            };
        }

        var findOwner = horse.HorseUsers.SingleOrDefault(u => u.IsOwner);

        if (findOwner == null)
        {
            horse.OwnerName = horseDto.OwnerName;
            horse.OwnerPhoneNumber = horseDto.OwnerPhoneNumber;
        }

        var createSaveResult = _saveService.Create(horseDto.Saves, horse, user.Id);

        if (!createSaveResult.Success)
        {
            return new ServiceResult<HorseDto>()
            {
                Success = false,
                ErrorMessage = createSaveResult.ErrorMessage
            };
        }

        _context.Horses.Add(horse);

        return new ServiceResult<HorseDto>()
        {
            Success = true,
            Result = await MapHorse(horse, user.Id),
        };
    }

    public async Task<ServiceResult<HorseDto>> UpdateAsync(User user, HorseUpdatingDto horseDto)
    {
        Horse? entity = await _context.Horses.Include(h => h.HorseUsers)
            .FirstOrDefaultAsync(h => h.HorseId == horseDto.HorseId);

        if (entity == null)
        {
            return new ServiceResult<HorseDto>()
            {
                Success = false,
                ErrorMessage = "Horse not found"
            };
        }

        entity.Name = horseDto.Name;
        entity.BirthDate = horseDto.BirthDate;
        entity.Sex = horseDto.Sex;
        entity.City = horseDto.City;
        entity.Region = horseDto.Region;
        entity.Country = horseDto.Country;
        entity.LastModifiedByUserId = user.Id;
        entity.LastModified = DateTime.UtcNow;

        var result = await _userHorseRepository.UpdateAsync(user.Id, entity.HorseId, horseDto.Users);

        if (!result.Success)
        {
            return new ServiceResult<HorseDto>()
            {
                Success = false,
                ErrorMessage = result.ErrorMessage
            };
        }

        _context.Entry(entity).State = EntityState.Modified;

        if (entity.HorseUsers.SingleOrDefault(u => u.IsOwner) == null)
        {
            entity.OwnerName = horseDto.OwnerName;
            entity.OwnerPhoneNumber = horseDto.OwnerPhoneNumber;
        }
        else
        {
            entity.OwnerName = null;
            entity.OwnerPhoneNumber = null;
        }

        return new ServiceResult<HorseDto>()
        {
            Success = true,
            Result = await MapHorse(entity, user.Id),
        };
    }

    public async Task<bool> DeleteAsync(long horseId, long userId)
    {
        var user = await _context.HorseUsers.SingleOrDefaultAsync(x => x.UserId == userId && x.HorseId == horseId);

        if (user == null)
        {
            return false;
        }

        if (Enum.Parse<UserAccessRole>(user.AccessRole, true) == UserAccessRole.Creator)
        {
            Horse horse = await _context.Horses.SingleAsync(h => h.HorseId == horseId);
            _context.Remove(horse);
        }
        else
        {
            _context.Remove(user);
        }

        return true;
    }

    public async Task<int?> SaveChangesAsync()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = 0;
            result += await _context.SaveChangesAsync();
            result += await _userHorseRepository.SaveChangesAsync();
            result += await _saveService.SaveChangesAsync();
            transaction.Commit();
            return result;
        }
        catch (Exception)
        {
            transaction.Rollback();
            return null;
        }
    }

    private async Task<HorseDto> MapHorse(Horse horse, long userId)
    {
        await _context.Entry(horse).Collection(h => h.HorseUsers).LoadAsync();
        await _context.Entry(horse).Reference(h => h.LastModifiedByUser).LoadAsync();

        HorseDto horseDto = new()
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
        };

        horseDto.CreatedBy = new()
        {
            Date = horse.CreationDate,
        };

        HorseUser createdUser = horse.HorseUsers.Single(x => x.AccessRole == UserAccessRole.Creator.ToString());

        horseDto.CreatedBy.User = new()
        {
            AccessRole = createdUser!.AccessRole,
            IsOwner = createdUser!.IsOwner,
            UserId = createdUser!.UserId,
            FirstName = createdUser.User.FirstName,
            LastName = createdUser.User.LastName,
            PhoneNumber = createdUser.User.PhoneNumber
        };

        horseDto.LastModifiedBy = new()
        {
            Date = horse.LastModified,
        };

        var lastUpdateUser = horse.HorseUsers.SingleOrDefault(x => x.UserId == horse.LastModifiedByUserId);

        horseDto.LastModifiedBy.User = new()
        {
            AccessRole = lastUpdateUser?.AccessRole ?? "None",
            IsOwner = lastUpdateUser?.IsOwner ?? false,
            UserId = horse.LastModifiedByUser?.Id ?? -1,
            FirstName = horse.LastModifiedByUser?.FirstName ?? "None",
            LastName = horse.LastModifiedByUser?.LastName,
            PhoneNumber = horse.LastModifiedByUser?.PhoneNumber
        };

        foreach (var user in horse.HorseUsers)
        {
            if (_context.Entry(user).State == EntityState.Deleted)
            {
                continue;
            }

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

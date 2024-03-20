using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Dtos.Response;
using Ford.WebApi.Models.Horse;
using Microsoft.EntityFrameworkCore;

namespace Ford.WebApi.Services;

public class UserHorseService : IUserHorseRepository
{
    private readonly FordContext _context;

    public UserHorseService(FordContext context)
    {
        _context = context;
    }

    public async Task<ICollection<HorseUserDto>> GetAsync(long horseId)
    {
        var users = _context.HorseUsers.Where(u => u.HorseId == horseId);
        return await MapHorseUsers(users.ToList());
    }

    public ResponseResult<Horse> Create(User user, Horse horse, ICollection<RequestHorseUser> requestHorseUsers)
    {
        //Find exist user in DB
        IEnumerable<User> containsUsers = _context.Users
            .Where(u => requestHorseUsers
            .Select(o => o.UserId)
            .Contains(u.Id));

        // find other user with creator access
        var secondCreator = requestHorseUsers.SingleOrDefault(u => u.UserId != user.Id &&
            Enum.Parse<UserAccessRole>(u.AccessRole) == UserAccessRole.Creator);

        if (secondCreator != null)
        {
            return new ResponseResult<Horse>
            {
                Success = false,
                ErrorMessage = "You cannot add user with creator access"
            };
        }

        if (containsUsers.Count() == requestHorseUsers.Count())
        {
            foreach (var reqUser in requestHorseUsers)
            {
                // add yourself
                if (reqUser.UserId == user.Id)
                {
                    horse.Users.Add(new()
                    {
                        UserId = reqUser.UserId,
                        AccessRole = UserAccessRole.Creator.ToString(),
                        IsOwner = reqUser.IsOwner,
                    });

                    continue;
                }

                if (!Enum.TryParse(reqUser.AccessRole, true, out UserAccessRole role))
                {
                    return new ResponseResult<Horse>
                    {
                        Success = false,
                        ErrorMessage = "Invalid Role",
                    };
                }

                horse.Users.Add(new()
                {
                    UserId = reqUser.UserId,
                    AccessRole = reqUser.AccessRole.ToString(),
                    IsOwner = reqUser.IsOwner,
                });
            }
        }
        else
        {
            return new ResponseResult<Horse>
            {
                Success = false,
                ErrorMessage = "Some users not found",
            };
        }

        // add yourserl
        if (!horse.Users.Any(u => u.UserId == user.Id))
        {
            horse.Users.Add(new()
            {
                UserId = user.Id,
                AccessRole = UserAccessRole.Creator.ToString(),
                IsOwner = false,
            });
        }

        return new ResponseResult<Horse>
        {
            Success = true,
            Result = horse
        };
    }

    public async Task<ResponseResult<ICollection<HorseUserDto>>> UpdateAsync(long currentUserId, long horseId, ICollection<RequestHorseUser> requestHorseUsers)
    {
        var users = _context.HorseUsers.Where(u => u.HorseId == horseId);
        var horseUser = await users.SingleOrDefaultAsync(u => u.UserId == currentUserId);

        // check access to action
        if (Enum.Parse<UserAccessRole>(horseUser!.AccessRole) < UserAccessRole.All)
        {
            return new ResponseResult<ICollection<HorseUserDto>>
            {
                Success = false,
                ErrorMessage = "You don't have permission for update or add users",
            };
        }

        // select users for delete
        var usersForDeleting = users.Where(u => !requestHorseUsers.Any(x => x.UserId == u.UserId));

        if (usersForDeleting.Any(u => Enum.Parse<UserAccessRole>(u.AccessRole) >= Enum.Parse<UserAccessRole>(horseUser!.AccessRole)))
        {
            return new ResponseResult<ICollection<HorseUserDto>>()
            {
                Success = false,
                ErrorMessage = "You cannot delete users who have equal or higher access then your"
            };
        }

        foreach (var userDelete in usersForDeleting)
        {
            _context.Entry(userDelete).State = EntityState.Deleted;
        }

        // select users for add
        var usersForAdding = requestHorseUsers.Where(u => !users.Any(x => x.UserId == u.UserId));

        if (usersForAdding.Any(u => Enum.Parse<UserAccessRole>(u.AccessRole) >= Enum.Parse<UserAccessRole>(horseUser!.AccessRole)))
        {
            return new ResponseResult<ICollection<HorseUserDto>>()
            {
                Success = false,
                ErrorMessage = "You cannot add users who have equal or higher access then your"
            };
        }

        foreach (var addingUser in usersForAdding)
        {
            _context.HorseUsers.Add(new()
            {
                HorseId = horseId,
                UserId = addingUser.UserId,
                AccessRole = addingUser.AccessRole,
                IsOwner = addingUser.IsOwner,
            });
        }

        // select users for update
        var userForChanging = requestHorseUsers.Where(u => !users.Any(x => 
            x.UserId == u.UserId && 
            x.IsOwner == u.IsOwner &&
            x.AccessRole == u.AccessRole));

        if (userForChanging.Any(u => Enum.Parse<UserAccessRole>(u.AccessRole) >= Enum.Parse<UserAccessRole>(horseUser!.AccessRole)))
        {
            return new ResponseResult<ICollection<HorseUserDto>>
            {
                Success = false,
                ErrorMessage = "You cannot set equal or higher access role then your"
            };
        }

        foreach (var changeUser in userForChanging)
        {
            var entity = users.Single(u => u.UserId == changeUser.UserId);

            entity.AccessRole = changeUser.AccessRole;
            entity.IsOwner = changeUser.IsOwner;
            _context.Entry(entity).State = EntityState.Modified;
        }

        // check one owner
        var owners = users.Where(u => u.IsOwner);

        if (owners.Count() > 1)
        {
            return new ResponseResult<ICollection<HorseUserDto>>
            {
                Success = false,
                ErrorMessage = "Cannot be more than one owner"
            };
        }

        await _context.SaveChangesAsync();
        return new ResponseResult<ICollection<HorseUserDto>>
        {
            Success = true,
            Result = await MapHorseUsers(users.ToList())
        };
    }

    public async Task<ResponseResult<Horse>> UpdateAsync(long currentUserId, Horse horse, ICollection<RequestHorseUser> requestUsers)
    {
        _context.Entry(horse).State = EntityState.Modified;
        await _context.Entry(horse).Collection(h => h.Users).LoadAsync();
        var horseUser = horse.Users.SingleOrDefault(u => u.UserId == currentUserId);

        // check access to action
        if (Enum.Parse<UserAccessRole>(horseUser!.AccessRole) < UserAccessRole.All)
        {
            return new ResponseResult<Horse>
            {
                Success = false,
                ErrorMessage = "You don't have permission for update or add users",
            };
        }

        // change self
        var self = requestUsers.SingleOrDefault(u => u.UserId == currentUserId);

        if (self == null)
        {
            return new ResponseResult<Horse>
            {
                Success = false,
                ErrorMessage = "You not found as horse's user in request body",
            };
        }

        horseUser.IsOwner = self.IsOwner;
        _context.Entry(horseUser).State = EntityState.Modified;

        // select users for delete
        var usersForDeleting = horse.Users.Where(u => !requestUsers.Any(x => x.UserId == u.UserId));

        if (usersForDeleting.Any(u => Enum.Parse<UserAccessRole>(u.AccessRole) >= Enum.Parse<UserAccessRole>(horseUser!.AccessRole)))
        {
            return new ResponseResult<Horse>()
            {
                Success = false,
                ErrorMessage = "You cannot delete users who have equal or higher access then your"
            };
        }

        foreach (var userDelete in usersForDeleting)
        {
            _context.Entry(userDelete).State = EntityState.Deleted;
        }

        // select users for add
        var usersForAdding = requestUsers.Where(u => !horse.Users.Any(x => x.UserId == u.UserId));

        if (usersForAdding.Any(u => Enum.Parse<UserAccessRole>(u.AccessRole) >= Enum.Parse<UserAccessRole>(horseUser!.AccessRole)))
        {
            return new ResponseResult<Horse>()
            {
                Success = false,
                ErrorMessage = "You cannot add users who have equal or higher access then your"
            };
        }

        foreach (var addingUser in usersForAdding)
        {
            horse.Users.Add(new()
            {
                UserId = addingUser.UserId,
                AccessRole = addingUser.AccessRole,
                IsOwner = addingUser.IsOwner,
            });
        }

        // select users for update
        var userForChanging = requestUsers.Where(u => !horse.Users.Any(x =>
            x.UserId == u.UserId &&
            x.IsOwner == u.IsOwner &&
            x.AccessRole == u.AccessRole));

        if (userForChanging.Any(u => Enum.Parse<UserAccessRole>(u.AccessRole) >= Enum.Parse<UserAccessRole>(horseUser!.AccessRole)))
        {
            return new ResponseResult<Horse>
            {
                Success = false,
                ErrorMessage = "You cannot set equal or higher access role then your"
            };
        }

        foreach (var changeUser in userForChanging)
        {
            var entity = horse.Users.Single(u => u.UserId == changeUser.UserId);

            entity.AccessRole = changeUser.AccessRole;
            entity.IsOwner = changeUser.IsOwner;
            _context.Entry(entity).State = EntityState.Modified;
        }

        // check one owner
        var owners = horse.Users.Where(u => u.IsOwner);

        if (owners.Count() > 1)
        {
            return new ResponseResult<Horse>
            {
                Success = false,
                ErrorMessage = "Cannot be more than one owner"
            };
        }

        await _context.SaveChangesAsync();
        return new ResponseResult<Horse>
        {
            Success = true,
            Result = horse
        };
    }

    public async Task<ResponseResult<ICollection<HorseUserDto>>> AddAsync(long currentUserId, long horseId, RequestHorseUser requestHorseUser)
    {
        var users = _context.HorseUsers.Where(u => u.HorseId == horseId);
        var horseUser = await _context.HorseUsers.SingleOrDefaultAsync(u => u.UserId == currentUserId && u.HorseId == horseId);

        if (Enum.Parse<UserAccessRole>(horseUser!.AccessRole) < UserAccessRole.All)
        {
            return new ResponseResult<ICollection<HorseUserDto>>
            {
                Success = false,
                ErrorMessage = "You don't have permission for update or add users",
            };
        }

        var existUser = await _context.HorseUsers.SingleOrDefaultAsync(u => u.UserId == requestHorseUser.UserId);

        if (existUser != null)
        {
            return new ResponseResult<ICollection<HorseUserDto>>
            {
                Success = false,
                ErrorMessage = "User is already exists"
            };
        }

        _context.HorseUsers.Add(new()
        {
            HorseId = horseId,
            UserId = requestHorseUser.UserId,
            AccessRole = requestHorseUser.AccessRole,
            IsOwner = requestHorseUser.IsOwner,
        });

        // check zero/one owner
        var owners = users.Where(u => u.IsOwner);

        if (owners.Count() > 1)
        {
            return new ResponseResult<ICollection<HorseUserDto>>
            {
                Success = false,
                ErrorMessage = "Cannot be more than one owner"
            };
        }

        // check exist yourself
        if (!users.Contains(horseUser))
        {
            return new ResponseResult<ICollection<HorseUserDto>>
            {
                Success = false,
                ErrorMessage = "You cannot remove yourself"
            };
        }

        await _context.SaveChangesAsync();
        return new ResponseResult<ICollection<HorseUserDto>>
        {
            Success = true,
            Result = await MapHorseUsers(users.ToList())
        };
    }

    public async Task<ResponseResult> DeleteAsync(long currentUserId, long horseId, long userId)
    {
        var users = _context.HorseUsers.Where(u => u.HorseId == horseId);
        var horseUser = await users.SingleOrDefaultAsync(u => u.UserId == currentUserId);

        // check access to action
        if (Enum.Parse<UserAccessRole>(horseUser!.AccessRole) < UserAccessRole.All)
        {
            return new ResponseResult<ICollection<HorseUserDto>>
            {
                Success = false,
                ErrorMessage = "You don't have permition for update or add users",
            };
        }

        var userDelete = await users.SingleOrDefaultAsync(u => u.UserId == userId);

        if (userDelete == null)
        {
            return new ResponseResult
            {
                Success = false,
                ErrorMessage = "User is not exists"
            };
        }

        if (Enum.Parse<UserAccessRole>(userDelete.AccessRole) >= Enum.Parse<UserAccessRole>(horseUser!.AccessRole))
        {
            return new ResponseResult
            {
                Success = false,
                ErrorMessage = "You cannot delete user who have equal or higher access then your"
            };
        }

        _context.HorseUsers.Remove(userDelete);

        // check exist yourself
        if (!users.Contains(horseUser))
        {
            return new ResponseResult<ICollection<HorseUserDto>>
            {
                Success = false,
                ErrorMessage = "You cannot remove yourself"
            };
        }

        await _context.SaveChangesAsync();
        return new ResponseResult { Success = true };
    }

    private async Task<ICollection<HorseUserDto>> MapHorseUsers(ICollection<UserHorse> users)
    {
        List<HorseUserDto> usersDto = [];

        foreach (var user in users)
        {
            var userRef = _context.Entry(user).Reference(u => u.User);
            await userRef.LoadAsync();

            usersDto.Add(new()
            {
                UserId = user.UserId,
                AccessRole = user.AccessRole,
                IsOwner = user.IsOwner,
                FirstName = user.User.FirstName,
                LastName = user.User.LastName,
                PhoneNumber = user.User.PhoneNumber,
            });
        }

        return usersDto;
    }
}

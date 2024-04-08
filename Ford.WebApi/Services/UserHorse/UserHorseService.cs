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

    public ServiceResult<ICollection<HorseUser>> Create(User user, long horseId, ICollection<RequestHorseUser> requestHorseUsers)
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
            return new ServiceResult<ICollection<HorseUser>>
            {
                Success = false,
                ErrorMessage = "You cannot add user with creator access"
            };
        }

        List<HorseUser> users = new();

        if (containsUsers.Count() == requestHorseUsers.Count())
        {
            foreach (var reqUser in requestHorseUsers)
            {
                // add yourself
                if (reqUser.UserId == user.Id)
                {
                    users.Add(new()
                    {
                        UserId = reqUser.UserId,
                        HorseId = horseId,
                        AccessRole = UserAccessRole.Creator.ToString(),
                        IsOwner = reqUser.IsOwner,
                    });

                    continue;
                }

                if (!Enum.TryParse(reqUser.AccessRole, true, out UserAccessRole role))
                {
                    return new ServiceResult<ICollection<HorseUser>>
                    {
                        Success = false,
                        ErrorMessage = "Invalid Role",
                    };
                }

                users.Add(new()
                {
                    UserId = reqUser.UserId,
                    HorseId = horseId,
                    AccessRole = reqUser.AccessRole.ToString(),
                    IsOwner = reqUser.IsOwner,
                });
            }
        }
        else
        {
            return new ServiceResult<ICollection<HorseUser>>
            {
                Success = false,
                ErrorMessage = "Some users not found",
            };
        }

        // add yourserl
        if (!users.Any(u => u.UserId == user.Id))
        {
            users.Add(new()
            {
                UserId = user.Id,
                HorseId = horseId,
                AccessRole = UserAccessRole.Creator.ToString(),
                IsOwner = false,
            });
        }

        return new ServiceResult<ICollection<HorseUser>>
        {
            Success = true,
            Result = users
        };
    }

    public ServiceResult<ICollection<HorseUser>> Create(User user, Horse horse, ICollection<RequestHorseUser> requestHorseUsers)
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
            return new ServiceResult<ICollection<HorseUser>>
            {
                Success = false,
                ErrorMessage = "You cannot add user with creator access"
            };
        }

        List<HorseUser> users = new();

        if (containsUsers.Count() == requestHorseUsers.Count())
        {
            foreach (var reqUser in requestHorseUsers)
            {
                // add yourself
                if (reqUser.UserId == user.Id)
                {
                    users.Add(new()
                    {
                        UserId = reqUser.UserId,
                        Horse = horse,
                        AccessRole = UserAccessRole.Creator.ToString(),
                        IsOwner = reqUser.IsOwner,
                    });

                    continue;
                }

                if (!Enum.TryParse(reqUser.AccessRole, true, out UserAccessRole role))
                {
                    return new ServiceResult<ICollection<HorseUser>>
                    {
                        Success = false,
                        ErrorMessage = "Invalid Role",
                    };
                }

                users.Add(new()
                {
                    UserId = reqUser.UserId,
                    Horse = horse,
                    AccessRole = reqUser.AccessRole.ToString(),
                    IsOwner = reqUser.IsOwner,
                });
            }
        }
        else
        {
            return new ServiceResult<ICollection<HorseUser>>
            {
                Success = false,
                ErrorMessage = "Some users not found",
            };
        }

        // add yourserl
        if (!users.Any(u => u.UserId == user.Id))
        {
            users.Add(new()
            {
                UserId = user.Id,
                Horse = horse,
                AccessRole = UserAccessRole.Creator.ToString(),
                IsOwner = false,
            });
        }

        _context.AddRange(users);

        return new ServiceResult<ICollection<HorseUser>>
        {
            Success = true,
            Result = users
        };
    }

    //public async Task<ServiceResult<ICollection<HorseUserDto>>> UpdateAsync(long currentUserId, long horseId, ICollection<RequestHorseUser> requestHorseUsers)
    //{
    //    var users = _context.HorseUsers.Where(u => u.HorseId == horseId);
    //    var horseUser = await users.SingleOrDefaultAsync(u => u.UserId == currentUserId);

    //    // check access to action
    //    if (Enum.Parse<UserAccessRole>(horseUser!.AccessRole) < UserAccessRole.All)
    //    {
    //        return new ServiceResult<ICollection<HorseUserDto>>
    //        {
    //            Success = false,
    //            ErrorMessage = "You don't have permission for update or add users",
    //        };
    //    }

    //    // select users for delete
    //    var usersForDeleting = users.Where(u => !requestHorseUsers.Any(x => x.UserId == u.UserId));

    //    if (usersForDeleting.Any(u => Enum.Parse<UserAccessRole>(u.AccessRole) >= Enum.Parse<UserAccessRole>(horseUser!.AccessRole)))
    //    {
    //        return new ServiceResult<ICollection<HorseUserDto>>()
    //        {
    //            Success = false,
    //            ErrorMessage = "You cannot delete users who have equal or higher access then your"
    //        };
    //    }

    //    foreach (var userDelete in usersForDeleting)
    //    {
    //        _context.Entry(userDelete).State = EntityState.Deleted;
    //    }

    //    // select users for add
    //    var usersForAdding = requestHorseUsers.Where(u => !users.Any(x => x.UserId == u.UserId));

    //    if (usersForAdding.Any(u => Enum.Parse<UserAccessRole>(u.AccessRole) >= Enum.Parse<UserAccessRole>(horseUser!.AccessRole)))
    //    {
    //        return new ServiceResult<ICollection<HorseUserDto>>()
    //        {
    //            Success = false,
    //            ErrorMessage = "You cannot add users who have equal or higher access then your"
    //        };
    //    }

    //    foreach (var addingUser in usersForAdding)
    //    {
    //        _context.HorseUsers.Add(new()
    //        {
    //            HorseId = horseId,
    //            UserId = addingUser.UserId,
    //            AccessRole = addingUser.AccessRole,
    //            IsOwner = addingUser.IsOwner,
    //        });
    //    }

    //    // select users for update
    //    var userForChanging = requestHorseUsers.Where(u => !users.Any(x => 
    //        x.UserId == u.UserId && 
    //        x.IsOwner == u.IsOwner &&
    //        x.AccessRole == u.AccessRole));

    //    if (userForChanging.Any(u => Enum.Parse<UserAccessRole>(u.AccessRole) >= Enum.Parse<UserAccessRole>(horseUser!.AccessRole)))
    //    {
    //        return new ServiceResult<ICollection<HorseUserDto>>
    //        {
    //            Success = false,
    //            ErrorMessage = "You cannot set equal or higher access role then your"
    //        };
    //    }

    //    foreach (var changeUser in userForChanging)
    //    {
    //        var entity = users.Single(u => u.UserId == changeUser.UserId);

    //        entity.AccessRole = changeUser.AccessRole;
    //        entity.IsOwner = changeUser.IsOwner;
    //        _context.Entry(entity).State = EntityState.Modified;
    //    }

    //    // check one owner
    //    var owners = users.Where(u => u.IsOwner);

    //    if (owners.Count() > 1)
    //    {
    //        return new ServiceResult<ICollection<HorseUserDto>>
    //        {
    //            Success = false,
    //            ErrorMessage = "Cannot be more than one owner"
    //        };
    //    }

    //    // await _context.SaveChangesAsync();
    //    return new ServiceResult<ICollection<HorseUserDto>>
    //    {
    //        Success = true,
    //        Result = await MapHorseUsers(users.ToList())
    //    };
    //}

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<ServiceResult<ICollection<HorseUser>>> UpdateAsync(long currentUserId, long horseId, ICollection<RequestHorseUser> requestUsers)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        var users = _context.HorseUsers.Where(x => x.HorseId == horseId);
        var horseUser = users.SingleOrDefault(u => u.UserId == currentUserId);
        var creator = users.SingleOrDefault(u => u.AccessRole == UserAccessRole.Creator.ToString());

        // check access to action
        if (Enum.Parse<UserAccessRole>(horseUser!.AccessRole) < UserAccessRole.All)
        {
            return new ServiceResult<ICollection<HorseUser>>
            {
                Success = false,
                ErrorMessage = "You don't have permission for update or add users",
            };
        }

        // add self
        var self = requestUsers.SingleOrDefault(u => u.UserId == horseUser.UserId);

        if (self == null)
        {
            requestUsers.Add(new()
            {
                AccessRole = horseUser.AccessRole,
                UserId = horseUser.UserId,
                IsOwner = horseUser.IsOwner
            });
        }

        // select users for delete
        var usersForDeleting = users.ToList().Where(u => !requestUsers.Any(x => x.UserId == u.UserId));

        foreach (var userDelete in usersForDeleting)
        {
            _context.Entry(userDelete).State = EntityState.Deleted;
        }

        // select users for add
        var usersForAdding = requestUsers.Where(u => !users.ToList().Any(x => x.UserId == u.UserId));

        foreach (var addingUser in usersForAdding)
        {
            _context.HorseUsers.Add(new()
            {
                UserId = addingUser.UserId,
                HorseId = horseId,
                AccessRole = addingUser.AccessRole,
                IsOwner = addingUser.IsOwner,
            });
        }

        // select users for update
        var userForChanging = requestUsers.Where(x => !usersForAdding.Any(u => u.UserId == x.UserId));

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
            return new ServiceResult<ICollection<HorseUser>>
            {
                Success = false,
                ErrorMessage = "Cannot be more than one owner"
            };
        }

        // check one or more creators
        var someCreators = users.ToList().Where(u => u.AccessRole == UserAccessRole.Creator.ToString());

        if (someCreators.Count() > 1)
        {
            foreach (var c in someCreators)
            {
                if (c.UserId != creator!.UserId)
                {
                    _context.Entry(c).State = EntityState.Deleted;
                }
            }
        }

        // check removing creator
        if (creator!.AccessRole != UserAccessRole.Creator.ToString())
        {
            creator!.AccessRole = UserAccessRole.Creator.ToString();
        }

        if (_context.Entry(creator!).State == EntityState.Deleted)
        {
            _context.Entry(creator!).State = EntityState.Modified;
            creator!.IsOwner = false;
        }

        return new ServiceResult<ICollection<HorseUser>>
        {
            Success = true,
            Result = _context.HorseUsers.Local.Where(x => x.HorseId == horseId && _context.Entry(x).State != EntityState.Deleted).ToList()
        };
    }

    public async Task<ServiceResult<ICollection<HorseUser>>> AddAsync(long currentUserId, long horseId, RequestHorseUser requestHorseUser)
    {
        var users = _context.HorseUsers.Where(u => u.HorseId == horseId);
        var horseUser = await _context.HorseUsers.SingleOrDefaultAsync(u => u.UserId == currentUserId && u.HorseId == horseId);

        if (Enum.Parse<UserAccessRole>(horseUser!.AccessRole) < UserAccessRole.All)
        {
            return new ServiceResult<ICollection<HorseUser>>
            {
                Success = false,
                ErrorMessage = "You don't have permission for update or add users",
            };
        }

        var existUser = await _context.HorseUsers.SingleOrDefaultAsync(u => u.UserId == requestHorseUser.UserId);

        if (existUser != null)
        {
            return new ServiceResult<ICollection<HorseUser>>
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
            return new ServiceResult<ICollection<HorseUser>>
            {
                Success = false,
                ErrorMessage = "Cannot be more than one owner"
            };
        }

        // check exist yourself
        if (!users.Contains(horseUser))
        {
            return new ServiceResult<ICollection<HorseUser>>
            {
                Success = false,
                ErrorMessage = "You cannot remove yourself"
            };
        }

        // await _context.SaveChangesAsync();
        return new ServiceResult<ICollection<HorseUser>>
        {
            Success = true,
            Result = [.. await users.AsNoTracking().ToListAsync()]
        };
    }

    public async Task<ServiceResult> DeleteAsync(long currentUserId, long horseId, long userId)
    {
        var users = _context.HorseUsers.Where(u => u.HorseId == horseId);
        var horseUser = await users.SingleOrDefaultAsync(u => u.UserId == currentUserId);

        // check access to action
        if (Enum.Parse<UserAccessRole>(horseUser!.AccessRole) < UserAccessRole.All)
        {
            return new ServiceResult<ICollection<HorseUserDto>>
            {
                Success = false,
                ErrorMessage = "You don't have permition for update or add users",
            };
        }

        var userDelete = await users.SingleOrDefaultAsync(u => u.UserId == userId);

        if (userDelete == null)
        {
            return new ServiceResult
            {
                Success = false,
                ErrorMessage = "User is not exists"
            };
        }

        if (Enum.Parse<UserAccessRole>(userDelete.AccessRole) >= Enum.Parse<UserAccessRole>(horseUser!.AccessRole))
        {
            return new ServiceResult
            {
                Success = false,
                ErrorMessage = "You cannot delete user who have equal or higher access then your"
            };
        }

        _context.HorseUsers.Remove(userDelete);

        // check exist yourself
        if (!users.Contains(horseUser))
        {
            return new ServiceResult<ICollection<HorseUserDto>>
            {
                Success = false,
                ErrorMessage = "You cannot remove yourself"
            };
        }

        // await _context.SaveChangesAsync();
        return new ServiceResult { Success = true };
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    private async Task<ICollection<HorseUserDto>> MapHorseUsers(ICollection<HorseUser> users)
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

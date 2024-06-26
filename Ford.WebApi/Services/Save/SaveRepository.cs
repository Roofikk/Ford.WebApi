﻿using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Request;
using Ford.WebApi.Dtos.Response;
using Ford.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Ford.WebApi.Services;

public class SaveRepository : ISaveRepository
{
    private FordContext _context;

    public SaveRepository(FordContext context)
    {
        _context = context;
    }

    public async Task<ICollection<SaveDto>> GetAsync(long horseId, long userId, int below = 0, int amount = 20)
    {
        List<SaveDto> savesDto = [];
        var saves = _context.Saves.Where(s => s.HorseId == horseId && s.Horse.HorseUsers.Any(o => o.UserId == userId))
            .OrderBy(o => o.LastModified)
            .Skip(below)
            .Take(amount);

        var savesList = await saves.ToListAsync();

        foreach (var save in saves)
        {
            SaveDto saveDto = await MapSave(save);
            savesDto.Add(saveDto);
        }

        return savesDto;
    }

    public async Task<FullSaveDto?> GetAsync(long horseId, long saveId, long userId)
    {
        var save = await _context.Saves.SingleOrDefaultAsync(s => s.SaveId == saveId);

        if (save == null)
        {
            return null;
        }

        var collection = _context.Entry(save).Collection(s => s.SaveBones);
        await collection.LoadAsync();

        FullSaveDto saveDto = await MapSave(save);
        return saveDto;
    }

    public ServiceResult<ICollection<Save>> Create(ICollection<HorseSaveCreatingDto> requestCreateSaves, Horse horse, long userId)
    {
        List<Save> saves = new();

        foreach (var saveDto in requestCreateSaves)
        {
            var save = new Save()
            {
                Horse = horse,
                Header = saveDto.Header,
                Description = saveDto.Description,
                Date = saveDto.Date,
                CreationDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                CreatedByUserId = userId,
                LastModifiedByUserId = userId,
            };

            foreach (var bone in saveDto.Bones)
            {
                if (bone.Position == null && bone.Rotation == null)
                {
                    continue;
                }

                var saveBone = new SaveBone()
                {
                    BoneId = bone.BoneId,
                };

                if (bone.Position != null)
                {
                    saveBone.PositionX = bone.Position.X;
                    saveBone.PositionY = bone.Position.Y;
                    saveBone.PositionZ = bone.Position.Z;
                }

                if (bone.Rotation != null)
                {
                    saveBone.RotationX = bone.Rotation.X;
                    saveBone.RotationY = bone.Rotation.Y;
                    saveBone.RotationZ = bone.Rotation.Z;
                }

                save.SaveBones.Add(saveBone);
            }

            saves.Add(save);
        }

        _context.Saves.AddRange(saves);
        return new ServiceResult<ICollection<Save>>()
        {
            Success = true,
            Result = saves
        };
    }

    public async Task<ServiceResult<ICollection<Save>>> CreateSavesToExistHorseAsync(ICollection<SaveCreatingDto> requestCreateSaves, long userId)
    {
        List<Save> saves = new();
        foreach (var save in requestCreateSaves)
        {
            var result = await CreateToExistHorseAsync(save, userId);

            if (!result.Success)
            {
                return new ServiceResult<ICollection<Save>>()
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage
                };
            }
            else
            {
                saves.Add(result.Result!);
            }
        }

        return new ServiceResult<ICollection<Save>>
        {
            Success = true,
            Result = saves
        };
    }

    public async Task<ServiceResult<Save>> CreateToExistHorseAsync(SaveCreatingDto requestSave, long userId)
    {
        var user = await _context.HorseUsers.SingleOrDefaultAsync(x => x.HorseId == requestSave.HorseId && x.UserId == userId);

        if (user == null)
        {
            return new ServiceResult<Save>()
            {
                Success = false,
                ErrorMessage = "User not found"
            };
        }

        if (Enum.Parse<UserAccessRole>(user.AccessRole) < UserAccessRole.Writer)
        {
            return new ServiceResult<Save>()
            {
                Success = false,
                ErrorMessage = "Access denied"
            };
        }

        var save = new Save()
        {
            HorseId = requestSave.HorseId,
            Header = requestSave.Header,
            Description = requestSave.Description,
            Date = requestSave.Date,
            CreationDate = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            CreatedByUserId = userId,
            LastModifiedByUserId = userId,
        };

        foreach (var bone in requestSave.Bones)
        {
            if (bone.Position == null && bone.Rotation == null)
            {
                continue;
            }

            var saveBone = new SaveBone()
            {
                BoneId = bone.BoneId,
            };

            if (bone.Position != null)
            {
                saveBone.PositionX = bone.Position.X;
                saveBone.PositionY = bone.Position.Y;
                saveBone.PositionZ = bone.Position.Z;
            }

            if (bone.Rotation != null)
            {
                saveBone.RotationX = bone.Rotation.X;
                saveBone.RotationY = bone.Rotation.Y;
                saveBone.RotationZ = bone.Rotation.Z;
            }

            save.SaveBones.Add(saveBone);
        }

        _context.Entry(save).State = EntityState.Added;

        return new ServiceResult<Save>
        {
            Success = true,
            Result = save,
        };
    }

    public async Task<ServiceResult<SaveDto>> UpdateAsync(SaveUpdatingDto requestSave, long userId)
    {
        var save = await _context.Saves
            .SingleOrDefaultAsync(s => s.SaveId == requestSave.SaveId);

        if (save == null)
        {
            return new ServiceResult<SaveDto>()
            {
                Success = false,
                ErrorMessage = "Save not found"
            };
        }

        await _context.Entry(save).Reference(s => s.Horse).LoadAsync();
        await _context.Entry(save.Horse).Collection(x => x.HorseUsers).LoadAsync();

        var currentUser = save.Horse.HorseUsers.SingleOrDefault(u => u.UserId == userId);

        if (currentUser == null)
        {
            return new ServiceResult<SaveDto>()
            {
                Success = false,
                ErrorMessage = "User not found"
            };
        }

        if (Enum.Parse<UserAccessRole>(currentUser.AccessRole) < UserAccessRole.Writer)
        {
            return new ServiceResult<SaveDto>()
            {
                Success = false,
                ErrorMessage = "User does not have permission to update this save"
            };
        }

        save.Header = requestSave.Header;
        save.Description = requestSave.Description;
        save.Date = requestSave.Date;
        save.LastModified = DateTime.UtcNow;
        save.LastModifiedByUserId = userId;

        _context.Entry(save).State = EntityState.Modified;
        var saveDto = await MapSave(save);
        return new ServiceResult<SaveDto>()
        {
            Success = true,
            Result = saveDto
        };
    }

    public async Task<bool> DeleteAsync(long saveId, long userId)
    {
        var save = await _context.Saves.SingleOrDefaultAsync(s => s.SaveId == saveId);

        if (save == null)
        {
            return false;
        }

        await _context.Entry(save).Reference(s => s.Horse).LoadAsync();
        await _context.Entry(save.Horse).Collection(x => x.HorseUsers).LoadAsync();

        var currentUser = save.Horse.HorseUsers.SingleOrDefault(u => u.UserId == userId);

        if (currentUser == null)
        {
            return false;
        }

        if (Enum.Parse<UserAccessRole>(currentUser.AccessRole) < UserAccessRole.Writer)
        {
            return false;
        }

        _context.Entry(save).State = EntityState.Deleted;
        return true;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    private async Task<FullSaveDto> MapSave(Save save)
    {
        FullSaveDto responseSaveDto = new()
        {
            HorseId = save.HorseId,
            SaveId = save.SaveId,
            Header = save.Header,
            Description = save.Description,
            Date = save.Date,
        };

        responseSaveDto.CreatedBy = new()
        {
            Date = save.CreationDate,
        };

        responseSaveDto.LastModifiedBy = new()
        {
            Date = save.LastModified
        };

        if (save.CreatedByUser == null)
        {
            await _context.Entry(save).Reference(s => s.CreatedByUser).LoadAsync();
        }

        if (save.CreatedByUser != null)
        {
            await _context.Entry(save.CreatedByUser).Collection(c => c.HorseUsers).LoadAsync();
            var createdByHorseUser = save.CreatedByUser.HorseUsers
                .SingleOrDefault(s => s.UserId == save.CreatedByUserId && s.HorseId == save.HorseId);

            responseSaveDto.CreatedBy.User = new()
            {
                UserId = save.CreatedByUser.Id,
                FirstName = save.CreatedByUser.FirstName,
                LastName = save.CreatedByUser.LastName,
                PhoneNumber = save.CreatedByUser.PhoneNumber,
                AccessRole = createdByHorseUser?.AccessRole.ToString() ?? "None",
                IsOwner = createdByHorseUser?.IsOwner ?? false,
            };
        }

        if (save.LastUpdatedByUser == null)
        {
            await _context.Entry(save).Reference(s => s.LastUpdatedByUser).LoadAsync();
        }

        if (save.LastUpdatedByUser != null)
        {
            await _context.Entry(save.LastUpdatedByUser).Collection(c => c.HorseUsers).LoadAsync();
            var lastUpdatedByHorseUser = save.LastUpdatedByUser.HorseUsers
                .SingleOrDefault(s => s.UserId == save.LastModifiedByUserId && s.HorseId == save.HorseId);

            responseSaveDto.LastModifiedBy.User = new()
            {
                UserId = save.LastUpdatedByUser.Id,
                FirstName = save.LastUpdatedByUser.FirstName,
                LastName = save.LastUpdatedByUser.LastName,
                PhoneNumber = save.LastUpdatedByUser.PhoneNumber,
                AccessRole = lastUpdatedByHorseUser?.AccessRole.ToString() ?? "None",
                IsOwner = lastUpdatedByHorseUser?.IsOwner ?? false,
            };
        }

        foreach (var bone in save.SaveBones)
        {
            responseSaveDto.Bones.Add(new BoneDto()
            {
                BoneId = bone.BoneId,

                Position = new Vector()
                {
                    X = bone.PositionX!.Value,
                    Y = bone.PositionY!.Value,
                    Z = bone.PositionZ!.Value
                },
                Rotation = new Vector()
                {
                    X = bone.RotationX!.Value,
                    Y = bone.RotationY!.Value,
                    Z = bone.RotationZ!.Value
                }
            });
        }

        return responseSaveDto;
    }
}

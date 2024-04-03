using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Request;
using Ford.WebApi.Dtos.Response;
using Ford.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Ford.WebApi.Services
{
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
            var saves = _context.Saves.Where(s => s.HorseId == horseId && s.Horse.Users.Any(o => o.UserId == userId))
                .OrderBy(o => o.LastUpdate)
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

        public ServiceResult<ICollection<Save>> Create(ICollection<SaveCreatingDto> requestCreateSaves, long userId)
        {
            List<Save> saves = new();
            foreach (var save in requestCreateSaves)
            {
                var result = Create(save, userId);

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

        public ServiceResult<Save> Create(SaveCreatingDto requestSave, long userId)
        {
            var save = new Save()
            {
                Header = requestSave.Header,
                Description = requestSave.Description,
                Date = requestSave.Date,
                CreationDate = DateTime.UtcNow,
                LastUpdate = DateTime.UtcNow,
                CreatedByUserId = userId,
                LastUpdatedByUserId = userId,
            };

            foreach (var bone in requestSave.Bones)
            {
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

        public async Task<ServiceResult<SaveDto>> CreateAsync(SaveCreatingDto requestSave, long userId)
        {
            UserHorse? owner = await _context.HorseUsers
                .SingleOrDefaultAsync(o => o.UserId == userId && o.HorseId == requestSave.HorseId);

            if (owner is null)
            {
                return new ServiceResult<SaveDto>()
                {
                    Success = false,
                    ErrorMessage = "User not found",
                };
            }

            UserAccessRole currentOwnerRole = Enum.Parse<UserAccessRole>(owner.AccessRole);

            if (currentOwnerRole < UserAccessRole.Write)
            {
                return new ServiceResult<SaveDto>()
                {
                    Success = false,
                    ErrorMessage = "User does not have write access",
                };
            }

            await _context.Entry(owner).Reference(o => o.Horse).LoadAsync();

            Save save = new()
            {
                Header = requestSave.Header,
                Description = requestSave.Description,
                Date = requestSave.Date,
                Horse = owner.Horse,
                CreationDate = DateTime.UtcNow,
                LastUpdate = DateTime.UtcNow,
                CreatedByUserId = userId,
                LastUpdatedByUserId = userId,
                HorseId = requestSave.HorseId,
            };

            foreach (var bone in requestSave.Bones)
            {
                save.SaveBones.Add(new SaveBone
                {
                    BoneId = bone.BoneId,
                    Save = save,

                    PositionX = bone.Position?.X,
                    PositionY = bone.Position?.Y,
                    PositionZ = bone.Position?.Z,

                    RotationX = bone.Rotation?.X,
                    RotationY = bone.Rotation?.Y,
                    RotationZ = bone.Rotation?.Z,
                });
            }

            owner.Horse.LastUpdate = DateTime.UtcNow;

            await _context.AddAsync(save);
            return new ServiceResult<SaveDto>()
            {
                Success = true,
                Result = await MapSave(save),
            };
        }

        public async Task<ServiceResult<SaveDto>> UpdateAsync(RequestUpdateSaveDto requestSave, long userId)
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
            await _context.Entry(save).Collection(s => s.Horse.Users).LoadAsync();

            var currentUser = save.Horse.Users.SingleOrDefault(u => u.UserId == userId);

            if (currentUser == null)
            {
                return new ServiceResult<SaveDto>()
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            if (Enum.Parse<UserAccessRole>(currentUser.AccessRole) < UserAccessRole.Write)
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
            save.LastUpdate = DateTime.UtcNow;
            save.LastUpdatedByUserId = userId;

            _context.Entry(save).State = EntityState.Modified;
            var saveDto = await MapSave(save);
            return new ServiceResult<SaveDto>()
            {
                Success = true,
                Result = saveDto
            };
        }

        public async Task<bool> DeleteAsync(long saveId)
        {
            var save = await _context.Saves.SingleOrDefaultAsync(s => s.SaveId == saveId);

            if (save == null)
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
            if (save.CreatedByUser == null)
            {
                await _context.Entry(save).Reference(s => s.CreatedByUser).LoadAsync();
            }

            FullSaveDto responseSaveDto = new()
            {
                HorseId = save.HorseId,
                SaveId = save.SaveId,
                Header = save.Header,
                Description = save.Description,
                Date = save.Date,
                LastUpdate = save.LastUpdate,
                CreationDate = save.CreationDate,
            };

            if (save.CreatedByUser != null)
            {
                await _context.Entry(save.CreatedByUser).Collection(c => c.HorseOwners).LoadAsync();
                var createdByHorseUser = save.CreatedByUser.HorseOwners
                    .SingleOrDefault(s => s.UserId == save.CreatedByUserId && s.HorseId == save.HorseId);

                responseSaveDto.CreatedByUser = new()
                {
                    UserId = save.CreatedByUser.Id,
                    FirstName = save.CreatedByUser.FirstName,
                    LastName = save.CreatedByUser.LastName,
                    PhoneNumber = save.CreatedByUser.PhoneNumber
                };

                if (createdByHorseUser != null)
                {
                    responseSaveDto.CreatedByUser.AccessRole = createdByHorseUser.AccessRole;
                    responseSaveDto.CreatedByUser.IsOwner = createdByHorseUser.IsOwner;
                }
            }

            if (save.LastUpdatedByUser == null)
            {
                await _context.Entry(save).Reference(s => s.LastUpdatedByUser).LoadAsync();
            }

            if (save.LastUpdatedByUser != null)
            {
                await _context.Entry(save.LastUpdatedByUser).Collection(c => c.HorseOwners).LoadAsync();
                var lastUpdatedByHorseUser = save.LastUpdatedByUser.HorseOwners
                    .SingleOrDefault(s => s.UserId == save.LastUpdatedByUserId && s.HorseId == save.HorseId);

                responseSaveDto.LastUpdatedUser = new()
                {
                    UserId = save.LastUpdatedByUser.Id,
                    FirstName = save.LastUpdatedByUser.FirstName,
                    LastName = save.LastUpdatedByUser.LastName,
                    PhoneNumber = save.LastUpdatedByUser.PhoneNumber
                };

                if (lastUpdatedByHorseUser != null)
                {
                    responseSaveDto.LastUpdatedUser.AccessRole = lastUpdatedByHorseUser.AccessRole;
                    responseSaveDto.LastUpdatedUser.IsOwner = lastUpdatedByHorseUser.IsOwner;
                }
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
}

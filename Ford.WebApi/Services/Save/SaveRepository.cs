using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Request;
using Ford.WebApi.Dtos.Response;
using Ford.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace Ford.WebApi.Services
{
    public class SaveRepository : ISaveRepository
    {
        private FordContext _context;

        public SaveRepository(FordContext context)
        {
            _context = context;
        }

        public async Task<ICollection<ResponseSaveDto>> GetAsync(long horseId, long userId, int below = 0, int amount = 20)
        {
            List<ResponseSaveDto> savesDto = [];
            var saves = _context.Saves.Where(s => s.HorseId == horseId && s.Horse.Users.Any(o => o.UserId == userId))
                .OrderBy(o => o.LastUpdate)
                .Skip(below)
                .Take(amount);

            var savesList = await saves.ToListAsync();

            foreach (var save in saves)
            {
                ResponseSaveDto saveDto = new()
                {
                    HorseId = save.HorseId,
                    SaveId = save.SaveId,
                    Header = save.Header,
                    Description = save.Description,
                    Date = save.Date,
                };
                savesDto.Add(saveDto);
            }

            return savesDto;
        }

        public async Task<ResponseFullSave?> GetAsync(long horseId, long saveId, long userId)
        {
            var save = await _context.Saves.SingleOrDefaultAsync(s => s.SaveId == saveId);

            if (save == null)
            {
                return null;
            }

            var collection = _context.Entry(save).Collection(s => s.SaveBones);
            await collection.LoadAsync();

            ResponseFullSave saveDto = MapSave(save);
            return saveDto;
        }

        public async Task<ResponseSaveDto?> CreateAsync(RequestCreateSaveDto requestSave, long userId)
        {
            UserHorse? owner = await _context.HorseUsers.SingleOrDefaultAsync(
            o => o.UserId == userId && o.HorseId == requestSave.HorseId);

            if (owner is null)
            {
                return null;
            }

            UserAccessRole currentOwnerRole = Enum.Parse<UserAccessRole>(owner.RuleAccess);

            if (currentOwnerRole < UserAccessRole.Write)
            {
                return null;
            }

            var reference = _context.Entry(owner).Reference(o => o.Horse);
            await reference.LoadAsync();
            Horse? horse = reference.CurrentValue;

            if (horse is null)
            {
                return null;
            }

            Save save = new()
            {
                Header = requestSave.Header,
                Description = requestSave.Description,
                Date = requestSave.Date,
                UserId = userId,
                Horse = horse,
                CreationDate = DateTime.UtcNow,
                LastUpdate = DateTime.UtcNow,
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

            horse.LastUpdate = DateTime.UtcNow;

            await _context.AddAsync(save);
            await _context.SaveChangesAsync();
            return MapSave(save);
        }

        public async Task<ResponseSaveDto?> UpdateAsync(RequestUpdateSaveDto requestSave, Save save)
        {
            var horseReference = _context.Entry(save).Reference(s => s.Horse);
            await horseReference.LoadAsync();

            if (!horseReference.IsLoaded)
            {
                return null;
            }

            // get owners by horse
            var collection = _context.Entry(horseReference.CurrentValue!).Collection(h => h.Users);
            await collection.LoadAsync();

            if (!collection.IsLoaded)
            {
                return null;
            }

            save.Header = requestSave.Header;
            save.Description = requestSave.Description;
            save.Date = requestSave.Date;
            save.LastUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            var saveDto = new ResponseSaveDto()
            {
                SaveId = save.SaveId,
                HorseId = save.HorseId,
                Header = save.Header,
                Description = save.Description,
                Date = save.Date,
                CreationDate = save.CreationDate,
                LastUpdate = save.LastUpdate,
            };
            return saveDto;
        }

        public async Task<bool> DeleteAsync(Save save)
        {
            _context.Remove(save);
            await _context.SaveChangesAsync();
            return true;
        }

        private ResponseFullSave MapSave(Save save)
        {
            ResponseFullSave responseSaveDto = new ResponseFullSave()
            {
                HorseId = save.HorseId,
                SaveId = save.SaveId,
                Header = save.Header,
                Description = save.Description,
                Date = save.Date,
                Bones = new Collection<BoneDto>()
            };

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

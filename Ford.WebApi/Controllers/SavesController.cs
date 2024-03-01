using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Response;
using Ford.WebApi.Models;
using Ford.WebApi.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using Ford.WebApi.Dtos.Request;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Ford.WebApi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SavesController : ControllerBase
{
    private readonly FordContext db;
    private readonly UserManager<User> userManager;

    public SavesController(FordContext db, UserManager<User> userManager)
    {
        this.db = db;
        this.userManager = userManager;
    }

    // GET: api/<SavesController>/{horseId}
    [HttpGet()]
    [ProducesResponseType(typeof(ResponseFullSave), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ResponseSaveDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get([Required] long horseId, long? saveId)
    {
        User? user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        IQueryable<Save> saves = db.Saves.Where(s => s.HorseId == horseId && 
            s.Horse.HorseOwners.Any(o => o.UserId == user.Id));

        List<ResponseSaveDto> savesDto = new();

        if (saveId == null)
        {
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

            return Ok(savesDto);
        } 
        else
        {
            var save = await saves.SingleOrDefaultAsync(s => s.SaveId == saveId);

            if (save == null)
            {
                return NotFound();
            }

            var collection = db.Entry(save).Collection(s => s.SaveBones);
            await collection.LoadAsync();

            ResponseFullSave saveDto = MapSave(save);
            return Ok(saveDto);
        }
    }

    // POST api/<SavesController>/{horseId}
    // Create
    [HttpPost()]
    public async Task<ActionResult<ResponseSaveDto>> Create([Required] long horseId, [FromBody] RequestCreateSaveDto requestSave)
    {
        // get authorize user
        User? user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        if (!requestSave.Bones.Any())
        {
            return BadRequest("Save object can not have empty list bones");
        }

        HorseOwner? owner = await db.HorseOwners.SingleOrDefaultAsync(o => o.UserId == user.Id && o.HorseId == horseId);

        if (owner is null)
        {
            return NotFound();
        }

        OwnerAccessRole currentOwnerRole = Enum.Parse<OwnerAccessRole>(owner.RuleAccess);

        if (currentOwnerRole < OwnerAccessRole.Write)
        {
            return BadRequest("No access to this action with the specified parameters");
        }

        var reference = db.Entry(owner).Reference(o => o.Horse);
        reference.Load();
        Horse? horse = reference.CurrentValue;

        if (horse is null)
        {
            return NotFound("Horse not found");
        }

        Save save = new Save
        {
            Header = requestSave.Header,
            Description = requestSave.Description,
            Date = requestSave.Date,
            User = user,
            Horse = horse,
        };

        ICollection<SaveBone> saveBones = new Collection<SaveBone>();

        foreach (var bone in requestSave.Bones)
        {
            saveBones.Add(new SaveBone
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

        await db.SaveBones.AddRangeAsync(saveBones);
        await db.AddAsync(save);
        await db.SaveChangesAsync();
        return Created($"api/[controller]?horseId={horseId}&saveId={save.SaveId}", MapSave(save));
    }

    // PUT api/<SavesController>/5
    // Update
    [HttpPut()]
    public async Task<ActionResult<ResponseSaveDto>> Update([FromBody] RequestUpdateSaveDto requestSave)
    {
        User? user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        Save? save = await db.Saves.SingleOrDefaultAsync(s => s.SaveId == requestSave.SaveId);

        if (save is null)
        {
            return NotFound();
        }

        // get horse by save
        var horseReference = db.Entry(save).Reference(s => s.Horse);
        await horseReference.LoadAsync();

        if (!horseReference.IsLoaded)
        {
            return BadRequest();
        }

        // get owners by horse
        var collection = db.Entry(horseReference.CurrentValue!).Collection(h => h.HorseOwners);
        await collection.LoadAsync();

        if (!collection.IsLoaded)
        {
            return BadRequest();
        }

        HorseOwner? owner = collection.CurrentValue!.SingleOrDefault(o => o.UserId == user.Id);

        if (owner is null)
        {
            return BadRequest("You don't have access to the horse");
        }

        if (Enum.Parse<OwnerAccessRole>(owner.RuleAccess) < OwnerAccessRole.Write)
        {
            return BadRequest("You don't have access to the horse");
        }

        save.Header = requestSave.Header;
        save.Description = requestSave.Description;
        save.Date = requestSave.Date;

        await db.SaveChangesAsync();
        var saveDto = new ResponseSaveDto()
        {
            SaveId = save.SaveId,
            HorseId = save.HorseId,
            Header = save.Header,
            Description = save.Description,
            Date = save.Date
        };
        return saveDto;
    }

    // DELETE api/[controller]?saveId=5
    [HttpDelete()]
    public async Task<IActionResult> Delete([Required] int saveId)
    {
        User? user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        Save? save = await db.Saves.SingleOrDefaultAsync(s => s.SaveId == saveId);

        if (save is null)
        {
            return NotFound();
        }

        // check access
        var saveReference = db.Entry(save).Reference(s => s.Horse);
        await saveReference.LoadAsync();

        if (!saveReference.IsLoaded)
        {
            return BadRequest();
        }

        var collection = db.Entry(saveReference.CurrentValue!).Collection(h => h.HorseOwners);
        await collection.LoadAsync();

        if (!collection.IsLoaded)
        {
            return BadRequest();
        }

        HorseOwner? owner = collection.CurrentValue!.SingleOrDefault(o => o.UserId == user.Id);

        if (owner is null)
        {
            return BadRequest("Owner not found");
        }

        if (Enum.Parse<OwnerAccessRole>(owner.RuleAccess, true) < OwnerAccessRole.Write)
        {
            return BadRequest("You don't have access to the action");
        }

        db.Remove(save);
        await db.SaveChangesAsync();
        return Ok();
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

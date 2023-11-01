using AutoMapper;
using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Save;
using Ford.WebApi.Models;
using Ford.WebApi.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ford.WebApi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SavesController : ControllerBase
{
    private readonly FordContext db;
    private readonly IMapper mapper;
    private readonly ITokenService tokenService;

    public SavesController(FordContext db, IMapper mapper, ITokenService tokenService)
    {
        this.db = db;
        this.mapper = mapper;
        this.tokenService = tokenService;
    }

    // GET: api/<SavesController>/{horseId}
    [HttpGet("{horseId}")]
    public async Task<ActionResult<IEnumerable<ResponseSaveDto>>> Get(long horseId)
    {
        string token = Request.Headers["Authorization"];
        User? user = await tokenService.GetUserByToken(token);

        if (user is null)
        {
            return Unauthorized();
        }

        IQueryable<Save> saves = db.Saves.Where(s => s.HorseId == horseId && 
            s.Horse.HorseOwners.Any(o => o.UserId == user.Id));

        List<ResponseSaveDto> savesDto = new ();

        foreach (var save in saves)
        {
            var collection = db.Entry(save).Collection(s => s.SaveBones);
            await collection.LoadAsync();

            if (collection.CurrentValue is null)
                continue;

            foreach (var saveBone in collection.CurrentValue)
            {
                db.Entry(saveBone).Reference(sb => sb.Bone).Load();
            }

            ResponseSaveDto saveDto = MapSave(save);
            savesDto.Add(saveDto);
        }

        return Ok(savesDto);
    }

    // GET api/<SavesController>/5
    [HttpGet("{horseId}/{saveId}")]
    public async Task<ActionResult<ResponseSaveDto>> Get(long horseId, long saveId)
    {
        var result = await Get(horseId);
        var okResult = (OkObjectResult)result.Result!;
        var saves = okResult.Value as IEnumerable<ResponseSaveDto>;

        if (!saves!.Any())
        {
            return NotFound();
        }

        ResponseSaveDto? save = saves!.FirstOrDefault(s => s.SaveId == saveId);

        if (save is null)
        {
            return NotFound();
        }

        return Ok(save);
    }

    // POST api/<SavesController>/{horseId}
    // Create
    [HttpPost("{horseId}")]
    public async Task<ActionResult<ResponseSaveDto>> Post([FromRoute] long horseId, [FromBody] RequestCreateSaveDto requestSave)
    {
        // get authorize user
        User? user = await tokenService.GetUserByToken(Request.Headers["Authorization"]);

        if (user is null)
        {
            return Unauthorized();
        }

        if (!requestSave.Bones.Any())
        {
            return BadRequest("Save object can not have empty list bones");
        }

        HorseOwner? owner = await db.HorseOwners.FirstOrDefaultAsync(o => o.UserId == user.Id && o.HorseId == horseId);

        if (owner is null)
        {
            return NotFound();
        }

        OwnerRole currentOwnerRole = Enum.Parse<OwnerRole>(owner.RuleAccess);

        if (currentOwnerRole < OwnerRole.Write)
        {
            return BadRequest("No access to this action with the specified parameters");
        }

        Horse? horse = db.Entry(owner).Reference(o => o.Horse).CurrentValue;

        if (horse is null)
        {
            return NotFound("Horse not found");
        }

        Save save = new Save
        {
            Header = requestSave.Header,
            Description = requestSave.Description,
            Date = requestSave.Date,
            Horse = horse,
        };

        IEnumerable<Bone> bones = db.Bones.AsEnumerable();

        foreach (var rb in requestSave.Bones)
        {
            Bone? bone = bones.FirstOrDefault(b => b.BoneId == rb.BoneId);

            bone ??= new Bone
                {
                    BoneId = rb.BoneId,
                    GroupId = rb.GroupId,
                    Name = rb.Name,
                };

            if (rb.Position is null && rb.Rotation is null)
            {
                continue;
            }

            if (rb.Position!.Magnitude < 0.0001f && rb.Rotation!.Magnitude < 0.0001f)
            {
                continue;
            }

            db.SaveBones.Add(new SaveBone()
            {
                Bone = bone,
                Save = save,
                PositionX = rb.Position?.X,
                PositionY = rb.Position?.Y,
                PositionZ = rb.Position?.Z,
                RotationX = rb.Rotation?.X,
                RotationY = rb.Rotation?.Y,
                RotationZ = rb.Rotation?.Z,
            });
        }

        await db.SaveChangesAsync();
        return Created($"api/[controller]/{horseId}/{save.SaveId}", MapSave(save));
    }

    // PUT api/<SavesController>/5
    // Update
    [HttpPut("{id}")]
    public async Task<ActionResult<ResponseSaveDto>> Put(int id, [FromBody] RequestUpdateSaveDto requestSave)
    {
        // get authorize user
        User? user = await tokenService.GetUserByToken(Request.Headers["Authorization"]);

        if (user is null)
        {
            return Unauthorized();
        }

        Save? save = await db.Saves.SingleOrDefaultAsync(s => s.SaveId == id);

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

        if (Enum.Parse<OwnerRole>(owner.RuleAccess) < OwnerRole.Write)
        {
            return BadRequest("You don't have access to the horse");
        }

        save.Header = requestSave.Header;
        save.Description = requestSave.Description;
        save.Date = requestSave.Date;

        await db.SaveChangesAsync();
        var saveDto = MapSave(save);
        return saveDto;
    }

    // DELETE api/<SavesController>/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        User? user = await tokenService.GetUserByToken(Request.Headers["Authorization"]);

        if (user is null)
        {
            return Unauthorized();
        }

        Save? save = await db.Saves.SingleOrDefaultAsync(s => s.SaveId == id);

        if (save is null)
        {
            return NotFound();
        }

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

        if (Enum.Parse<OwnerRole>(owner.RuleAccess, true) < OwnerRole.Write)
        {
            return BadRequest("You don't have access to the action");
        }

        db.Remove(save);
        await db.SaveChangesAsync();
        return Ok();
    }

    private ResponseSaveDto MapSave(Save save)
    {
        ResponseSaveDto responseSaveDto = new ResponseSaveDto()
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
                GroupId = bone.Bone.GroupId,
                Name = bone.Bone.Name,
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

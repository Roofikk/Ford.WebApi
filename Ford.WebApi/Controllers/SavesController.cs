﻿using AutoMapper;
using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Save;
using Ford.WebApi.Models;
using Ford.WebApi.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace Ford.WebApi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SavesController : ControllerBase
{
    private readonly FordContext db;
    private readonly ITokenService tokenService;

    public SavesController(FordContext db, ITokenService tokenService)
    {
        this.db = db;
        this.tokenService = tokenService;
    }

    // GET: api/<SavesController>/{horseId}
    [HttpGet("{horseId}")]
    public async Task<ActionResult<IEnumerable<ResponseSaveDto>>> Get(long horseId)
    {
        User? user = await tokenService.GetUserByPrincipal(User);

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
        User? user = await tokenService.GetUserByPrincipal(User);

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

                PositionX = bone.Position?.X,
                PositionY = bone.Position?.Y,
                PositionZ = bone.Position?.Z,

                RotationX = bone.Rotation?.X,
                RotationY = bone.Rotation?.Y,
                RotationZ = bone.Rotation?.Z,
            });
        }

        save.SaveBones = saveBones;

        db.UpdateRange(save.SaveBones);
        await db.Saves.AddAsync(save);
        await db.SaveChangesAsync();
        return Created($"api/[controller]/{horseId}/{save.SaveId}", MapSave(save));
    }

    // PUT api/<SavesController>/5
    // Update
    [HttpPut("{id}")]
    public async Task<ActionResult<ResponseSaveDto>> Put(int id, [FromBody] RequestUpdateSaveDto requestSave)
    {
        User? user = await tokenService.GetUserByPrincipal(User);

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

        if (Enum.Parse<OwnerAccessRole>(owner.RuleAccess) < OwnerAccessRole.Write)
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
        User? user = await tokenService.GetUserByPrincipal(User);

        if (user is null)
        {
            return Unauthorized();
        }

        Save? save = await db.Saves.SingleOrDefaultAsync(s => s.SaveId == id);

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

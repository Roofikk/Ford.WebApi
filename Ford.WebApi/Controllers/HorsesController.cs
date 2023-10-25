﻿using AutoMapper;
using Ford.WebApi.Dtos.Horse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Data;
using Microsoft.AspNetCore.Authorization;
using Ford.WebApi.Services.Identity;
using Microsoft.Extensions.Primitives;
using Ford.WebApi.Models.Horse;
using System.Collections.ObjectModel;

namespace Ford.WebApi.Controllers;

[Authorize()]
[Route("api/[controller]")]
[ApiController]
public class HorsesController : ControllerBase
{
    private readonly FordContext db;
    private readonly IMapper mapper;
    private readonly ITokenService tokenService;

    public HorsesController(FordContext db, IMapper mapper, ITokenService tokenService)
    {
        this.db = db;
        this.mapper = mapper;
        this.tokenService = tokenService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(long? horseId)
    {
        if (!Request.Headers.TryGetValue("Authorization", out StringValues token))
        {
            return Unauthorized();
        }

        User? user = await tokenService.GetUserByToken(token);

        if (user is null)
        {
            return BadRequest();
        }

        IQueryable<Horse> dbHorses = db.Horses.Include(h => h.Saves);
        IEnumerable<Horse> horses = dbHorses.Where(h => h.HorseOwners.Any(u => u.UserId == user.Id));

        if (horses.Any())
        {
            if (horseId is not null)
            {
                Horse? horse = horses.FirstOrDefault(h => h.HorseId == horseId);
                HorseRetrievingDto horseDto = mapper.Map<HorseRetrievingDto>(horse);
                return Ok(horseDto);
            }
            else
            {
                IEnumerable<HorseRetrievingDto> horsesDto = mapper.Map<IEnumerable<HorseRetrievingDto>>(horses);
                return Ok(horses);
            }
        }
        else
        {
            return NoContent();
        }
    }

    //[HttpGet]
    //public async Task<IActionResult> Get(long? horseId)
    //{
    //    IQueryable<Horse> horsesDb = db.Horses.Include(h => h.HorseOwners).Include(h => h.Saves);

    //    if (horseId is not null)
    //    {
    //        var horse = await horsesDb.FirstOrDefaultAsync(h => h.HorseId == horseId);

    //        if (horse is not null)
    //        {
    //            HorseRetrievingDto mappingHorse = mapper.Map<HorseRetrievingDto>(horse);
    //            return Ok(horse);
    //        }
    //        else
    //        {
    //            return NotFound();
    //        }
    //    }
    //    else
    //    {
    //        var horses = await horsesDb.ToListAsync();
    //        IEnumerable<HorseRetrievingDto> mapping = mapper.Map<IEnumerable<HorseRetrievingDto>>(horses);
    //        return Ok(mapping);
    //    }
    //}

    [HttpPost]
    public async Task<ActionResult<Horse>> Create([FromBody] HorseForCreationDto horse)
    {
        if (!Request.Headers.TryGetValue("Authorization", out var token))
        {
            return Unauthorized();
        }

        User? user = await tokenService.GetUserByToken(token);

        if (user is null)
        {
            return BadRequest();
        }

        Horse horseDto = mapper.Map<Horse>(horse);
        IEnumerable<User> intersect = db.Users.IntersectBy(horse.HorseOwners.Select(e => e.UserId), u => u.Id);

        if (intersect.Any() && intersect.Count() == horse.HorseOwners.Count())
        {
            db.Horses.Add(horseDto);

            HorseOwner? owner = horse.HorseOwners.FirstOrDefault(
                hw => hw.UserId == user.Id && hw.HorseId == horseDto.HorseId);

            if (owner is null)
            {
                db.HorseOwners.Add(new HorseOwner
                {
                    Horse = horseDto,
                    UserId = user.Id,
                    HorseId = horseDto.HorseId,
                    RuleAccess = OwnerRole.Creator.ToString()
                });
            }

            foreach (var horseOwner in horse.HorseOwners)
            {
                OwnerRole role = Enum.Parse<OwnerRole>(horseOwner.RuleAccess, true);

                switch (role)
                {
                    case OwnerRole.Read:
                        horseOwner.RuleAccess = OwnerRole.Read.ToString();
                        break;
                    case OwnerRole.Write:
                        horseOwner.RuleAccess = OwnerRole.Write.ToString();
                        break;
                    case OwnerRole.All:
                        horseOwner.RuleAccess = OwnerRole.All.ToString();
                        break;
                    default:
                        string login = intersect.First(u => u.Id == horseOwner.UserId).UserName;
                        return BadRequest($"Role {role} does not apply to {login}");
                }

                horseDto.HorseOwners.Add(horseOwner);
            }

            bool result = (await db.SaveChangesAsync()) == 1;

            if (result)
            {
                HorseRetrievingDto horseRetrievingDto = mapper.Map<HorseRetrievingDto>(horseDto);
                return Created($"api/[controller]?horseId={horseRetrievingDto.HorseId}", horseRetrievingDto);
            }
            else
            {
                return BadRequest();
            }
        }
        else
        {
            return NotFound("Some users not found");
        }
    }

    // Не доделал. Надо же еще удалить прошлые, которые не были включены в список!
    // Надо было просто у лошади взять список владельцев и изменить его на текущий с текущими параметрами...
    // Мдааа... Херни наворотил.
    [HttpPost]
    [Route("horseOwners")]
    public async Task<ActionResult<Horse>> UpdateHorseOwnersAsync([FromBody] RequestUpdateHorseOwners requestHorseOwners)
    {
        if (!Request.Headers.TryGetValue("Authorization", out var token))
        {
            return Unauthorized();
        }

        User? user = await tokenService.GetUserByToken(token);

        if (user is null)
        {
            return BadRequest();
        }

        //Search existing horse owner
        HorseOwner? currentOwner = await db.HorseOwners.FirstOrDefaultAsync(
            hw => hw.UserId == user.Id && hw.HorseId == requestHorseOwners.HorseId);

        if (currentOwner is null)
        {
            return BadRequest("You do not have access to this object");
        }

        //Check the possibility of granting role to an object
        OwnerRole role = Enum.Parse<OwnerRole>(currentOwner.RuleAccess, true);
        var check = requestHorseOwners.HorseOwners.Where(hw => Enum.Parse<OwnerRole>(hw.RuleAccess) > role - 1);

        if (check.Any())
        {
            return BadRequest("Some rules upper than you may provide");
        }

        Horse? horse = await db.Horses.Include(h => h.HorseOwners)
            .FirstOrDefaultAsync(h => h.HorseId == requestHorseOwners.HorseId);

        if (horse is null)
        {
            return BadRequest("Horse not found");
        }

        Collection<HorseOwner> newOwners = new();

        if (requestHorseOwners.HorseOwners.FirstOrDefault(o => o.UserId == user.Id) is null)
        {
            newOwners.Add(new HorseOwner
            {
                UserId = user.Id,
                HorseId = requestHorseOwners.HorseId
            });
        }

        foreach (var reqOwner in requestHorseOwners.HorseOwners)
        {
            newOwners.Add(new HorseOwner
            {
                UserId = reqOwner.UserId,
                HorseId = requestHorseOwners.HorseId
            });
        }

        horse.HorseOwners = newOwners;

        await db.SaveChangesAsync();

        return horse;
    }

    [HttpPut]
    public async Task<ActionResult<Horse>> UpdateAsync([FromBody] HorseForUpdateDto horse)
    {
        if (!Request.Headers.TryGetValue("Authorization", out var token))
        {
            return Unauthorized();
        }

        User? user = await tokenService.GetUserByToken(token);

        if (user is null)
        {
            return BadRequest();
        }

        bool? check = await CheckAccessToHorseAsync(user.Id, horse.HorseId, OwnerRole.Write);

        if (!check.HasValue)
        {
            return BadRequest();
        }
        else if (!check.Value)
        {
            return BadRequest("Access denied");
        }

        Horse? entity = await db.Horses.Include(h => h.HorseOwners)
            .FirstOrDefaultAsync(h => h.HorseId == horse.HorseId);

        if (entity is null)
        {
            return BadRequest("Horse not found");
        }

        entity.Name = horse.Name;
        entity.BirthDate = horse.BirthDate;
        entity.Sex = horse.Sex;
        entity.City = horse.City;
        entity.Region = horse.Region;
        entity.Country = horse.Country;

        return await UpdateHorseOwnersAsync(new RequestUpdateHorseOwners
        {
            HorseId = entity.HorseId,
            HorseOwners = horse.Owners
        });
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAsync(long id)
    {
        Horse? horse = await db.Horses.FirstOrDefaultAsync(h => h.Equals(id));

        if (horse is not null)
        {
            db.Remove(horse);
            return Ok();
        }
        else
        {
            return NotFound();
        }
    }

    private async Task<HorseOwner?> GetHorseOwnerAsync(long userId, long horseId)
    {
        HorseOwner? owner = await db.HorseOwners.FirstOrDefaultAsync(
            o => o.UserId == userId && o.HorseId == horseId);

        return owner;
    }

    private bool? CheckAccessToHorse(HorseOwner? owner, OwnerRole needRole)
    {
        if (owner is null)
        {
            return null;
        }

        OwnerRole currentRole = Enum.Parse<OwnerRole>(owner.RuleAccess, true);

        return needRole >= currentRole;
    }

    private async Task<bool?> CheckAccessToHorseAsync(long userId, long horseId, OwnerRole needRole)
    {
        HorseOwner? owner = await GetHorseOwnerAsync(userId, horseId);
        return CheckAccessToHorse(owner, needRole);
    }
}

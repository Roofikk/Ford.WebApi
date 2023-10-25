using AutoMapper;
using Ford.WebApi.Dtos.Horse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Data;
using Microsoft.AspNetCore.Authorization;
using Ford.WebApi.Services.Identity;
using Microsoft.Extensions.Primitives;
using Ford.WebApi.Models.Horse;

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
    public async Task<ActionResult<Horse>> UpdateHorseOwners([FromBody] RequestUpdateHorseOwners requestHorseOwners)
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

        IEnumerable<User> intersect = db.Users.IntersectBy(requestHorseOwners.HorseOwners
            .Select(hw => hw.UserId), u => u.Id).AsEnumerable();

        if (!intersect.Any() || intersect.Count() != requestHorseOwners.HorseOwners.Count())
        {
            return BadRequest("Some users do not exist");
        }

        IEnumerable<HorseOwner> includesOwners = db.HorseOwners.Where(o => o.HorseId == requestHorseOwners.HorseId);

        foreach (var newOwner in requestHorseOwners.HorseOwners)
        {
            var existHw = includesOwners.FirstOrDefault(o => o.UserId == newOwner.UserId);

            if (existHw is null)
            {
                db.HorseOwners.Add(new HorseOwner
                {
                    HorseId = requestHorseOwners.HorseId,
                    UserId = newOwner.UserId,
                    RuleAccess = newOwner.RuleAccess
                });
            }
            else
            {
                existHw.RuleAccess = newOwner.RuleAccess;
            }
        }

        return Ok();

        //if ()
        //{
        //    horse.HorseOwners.Add(requestHorseOwners);
        //    int result = await db.SaveChangesAsync();

        //    if (result == 1)
        //    {
        //        HorseRetrievingDto horseDto = mapper.Map<HorseRetrievingDto>(horse);
        //        return Ok(horseDto);
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //}
        //else
        //{
        //    return NotFound();
        //}
    }

    [HttpPut]
    public async Task<ActionResult<Horse>> Update([FromBody]HorseForUpdateDto horse)
    {
        Horse? entity = await db.Horses.FirstOrDefaultAsync(h => h.HorseId == horse.HorseId);
        Horse horseDto = mapper.Map<Horse>(horse);

        if (entity is null)
        {
            return NotFound();
        }

        horseDto.CreationDate = entity.CreationDate;
        db.Entry(entity).CurrentValues.SetValues(horseDto);
        return Ok(entity);
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(long id)
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
}

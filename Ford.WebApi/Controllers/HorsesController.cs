using AutoMapper;
using Ford.WebApi.Dtos.Horse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Data;
using Microsoft.AspNetCore.Authorization;
using Ford.WebApi.Services.Identity;
using Microsoft.Extensions.Primitives;

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
        Horse horseDto = mapper.Map<Horse>(horse);
        IEnumerable<User> intersect = db.Users.IntersectBy(horse.HorseOwners.Select(e => e.UserId), e => e.Id);

        if (!Request.Headers.TryGetValue("Authorization", out var token))
        {
            return Unauthorized();
        }

        User? user = await tokenService.GetUserByToken(token);

        if (user is null)
        {
            return BadRequest();
        }

        if (intersect.Any() && intersect.Count() == horse.HorseOwners.Count())
        {
            db.Horses.Add(horseDto);

            HorseOwner? owner = horse.HorseOwners.FirstOrDefault(hw => hw.UserId == user.Id && hw.HorseId == horseDto.HorseId);

            if (owner is null)
            {
                db.HorseOwners.Add(new HorseOwner
                {
                    Horse = horseDto,
                    UserId = user.Id,
                    HorseId = horseDto.HorseId,
                    RuleAccess = HorseRuleAccess.Owner
                });
            }

            foreach (var horseOwner in horse.HorseOwners)
            {
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

    [HttpPost]
    [Route("horseAccess")]
    public async Task<ActionResult<Horse>> UpdateUserAccess([FromBody]HorseOwner horseOwner)
    {
        IQueryable<Horse> query = db.Horses.Include(h => h.HorseOwners);
        Horse? horse = await query.FirstOrDefaultAsync(h => h.HorseId == horseOwner.HorseId);
        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == horseOwner.UserId);

        if (horse is not null && user is not null)
        {
            horse.HorseOwners.Add(horseOwner);
            int result = await db.SaveChangesAsync();

            if (result == 1)
            {
                HorseRetrievingDto horseDto = mapper.Map<HorseRetrievingDto>(horse);
                return Ok(horseDto);
            }
            else
            {
                return BadRequest();
            }
        }
        else
        {
            return NotFound();
        }
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

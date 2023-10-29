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
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
    public async Task<ActionResult<HorseRetrievingDto[]>> Get()
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

        IEnumerable<Horse> horses = db.Horses.Where(h => h.HorseOwners.Any(o => o.UserId == user.Id))
            .AsEnumerable();

        if (!horses.Any())
        {
            return NotFound();
        }
        else
        {
            foreach (var horse in horses)
            {
                if (horse is null)
                {
                    return NotFound();
                }

                CollectionEntry<Horse, HorseOwner> collection = db.Entry(horse).Collection(h => h.HorseOwners);
                collection.Load();

                if (collection.CurrentValue is not null)
                {
                    foreach (var owner in collection.CurrentValue)
                    {
                        db.Entry(owner).Reference(o => o.User).Load();
                    }
                }
            }

            IEnumerable<HorseRetrievingDto> horsesDto = mapper.Map<IEnumerable<HorseRetrievingDto>>(horses);
            return horsesDto.ToArray();
        }
    }

    [HttpGet("{horseId}")]
    public async Task<ActionResult<HorseRetrievingDto>> Get(long horseId)
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

        Horse? horse = db.Horses.SingleOrDefault(h => h.HorseOwners.Any(u => u.UserId == user.Id) && h.HorseId == horseId);

        if (horse is null)
        {
            return NotFound();
        }

        CollectionEntry<Horse, HorseOwner> collection = db.Entry(horse).Collection(h => h.HorseOwners);
        collection.Load();

        if (collection.CurrentValue is not null)
        {
            foreach (var owner in collection.CurrentValue)
            {
                db.Entry(owner).Reference(o => o.User).Load();
            }
        }

        HorseRetrievingDto horseDto = mapper.Map<HorseRetrievingDto>(horse);
        return horseDto;
    }

    [HttpPost]
    public async Task<ActionResult<HorseRetrievingDto>> Create([FromBody] HorseForCreationDto requestHorse)
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

        Horse horseDto = mapper.Map<Horse>(requestHorse);
        
        //Add current user
        horseDto.HorseOwners.Add(new HorseOwner
        {
            Horse = horseDto,
            User = user,
            RuleAccess = OwnerRole.Creator.ToString()
        });

        if (requestHorse.HorseOwners is not null)
        {
            //Check the possibility of granting role to an object
            var check = requestHorse.HorseOwners.Where(hw => Enum.Parse<OwnerRole>(hw.RuleAccess) >= OwnerRole.Creator);

            if (check.Any())
            {
                return BadRequest("Some roles access can not be greater or equal than your");
            }

            //Find exist user in DB
            IEnumerable<User> containsUsers = db.Users.Include(u => u.HorseOwners)
                .Where(u => requestHorse.HorseOwners
                .Select(o => o.UserId).Contains(u.Id));

            if (containsUsers.Any() && containsUsers.Count() == requestHorse.HorseOwners.Count())
            {
                foreach (var horseOwner in requestHorse.HorseOwners)
                {
                    //Skip current user which was added early
                    if (horseOwner.UserId == user.Id)
                        continue;

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
                            string login = containsUsers.First(u => u.Id == horseOwner.UserId).UserName;
                            return BadRequest($"Role {role} does not apply to {login}");
                    }

                    horseDto.HorseOwners.Add(new HorseOwner
                    {
                        Horse = horseDto,
                        UserId = horseOwner.UserId,
                        RuleAccess = horseOwner.RuleAccess.ToString(),
                    });
                }
            }
            else
            {
                return NotFound("Some users not found");
            }
        }

        db.Horses.Add(horseDto);
        int number = await db.SaveChangesAsync();

        if (number == (requestHorse.HorseOwners)?.Count() + 2)
        {
            var horseRetrieving = mapper.Map<HorseRetrievingDto>(horseDto);
            return Created($"api/[controller]?horseId={horseDto.HorseId}", horseRetrieving);
        }
        else
        {
            return BadRequest("Save failed");
        }
    }

    // Не доделал. Надо же еще удалить прошлые, которые не были включены в список!
    // Надо было просто у лошади взять список владельцев и изменить его на текущий с текущими параметрами...
    // Мдааа... Херни наворотил.
    [HttpPost]
    [Route("horseOwners")]
    public async Task<ActionResult<HorseRetrievingDto>> UpdateHorseOwnersAsync([FromBody] RequestUpdateHorseOwners requestHorseOwners)
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

        OwnerRole currentOwnerRole = Enum.Parse<OwnerRole>(currentOwner.RuleAccess, true);

        //Check access to update
        if (currentOwnerRole < OwnerRole.Write)
        {
            return BadRequest("Access denied");
        }

        //Find invalid access role to an object
        var invalidRoles = requestHorseOwners.HorseOwners.Where(
            hw => Enum.Parse<OwnerRole>(hw.RuleAccess) > currentOwnerRole - 1);

        if (invalidRoles.Any())
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

        foreach (var reqOwner in requestHorseOwners.HorseOwners)
        {
            if (reqOwner.UserId == user.Id)
                continue;

            newOwners.Add(new HorseOwner
            {
                UserId = reqOwner.UserId,
                HorseId = requestHorseOwners.HorseId,
                RuleAccess = reqOwner.RuleAccess
            });
        }

        newOwners.Add(currentOwner);

        horse.HorseOwners = newOwners;
        await db.SaveChangesAsync();

        return await Get(horse.HorseId);
    }

    [HttpPut]
    public async Task<ActionResult<HorseRetrievingDto>> UpdateAsync([FromBody] HorseForUpdateDto horse)
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
    public async Task<IActionResult> DeleteAsync(long id)
    {
        if (!Request.Headers.TryGetValue("Authorization", out var token))
        {
            return Unauthorized();
        }

        User? user = await tokenService.GetUserByToken(token);

        if (user is null)
        {
            return Unauthorized();
        }

        Horse? horse = await db.Horses.Include(h => h.HorseOwners)
            .FirstOrDefaultAsync(h => h.HorseId == id);

        if (horse is null)
        {
            return NotFound();
        }

        HorseOwner? owner = horse.HorseOwners.SingleOrDefault(o => o.UserId == user.Id);

        if (owner is null)
        {
            return BadRequest("Horse not found in your list");
        }

        if (Enum.Parse<OwnerRole>(owner.RuleAccess, true) != OwnerRole.Creator)
        {
            return BadRequest("Access denied");
        }

        db.Remove(horse);
        await db.SaveChangesAsync();
        return Ok();
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

        return currentRole >= needRole;
    }

    private async Task<bool?> CheckAccessToHorseAsync(long userId, long horseId, OwnerRole needRole)
    {
        HorseOwner? owner = await GetHorseOwnerAsync(userId, horseId);
        return CheckAccessToHorse(owner, needRole);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Data;
using Microsoft.AspNetCore.Authorization;
using Ford.WebApi.Models.Horse;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Ford.WebApi.Dtos.Response;
using Ford.WebApi.Dtos.Horse;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net;
using Ford.WebApi.Dtos.Request;
using Microsoft.AspNetCore.Identity;

namespace Ford.WebApi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class HorsesController : ControllerBase
{
    private readonly FordContext db;
    private readonly UserManager<User> userManager;

    public HorsesController(FordContext db, UserManager<User> userManager)
    {
        this.db = db;
        this.userManager = userManager;
    }

    [HttpGet()]
    [ProducesResponseType(typeof(RetrieveArray<HorseRetrievingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HorseRetrievingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetAsync(long? horseId, int beginSelection = 0, int count = 20)
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Unauthorized",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("Unauthorized", "User unauthorized") }));
        }

        if (horseId == null)
        {
            IEnumerable<Horse> horses = db.Horses
                .Where(h => h.HorseOwners.Any(o => o.UserId == user.Id))
                .Include(h => h.HorseOwners)
                .ThenInclude(o => o.User)
                .Skip(beginSelection)
                .Take(count)
                .AsEnumerable();

            List<HorseRetrievingDto> horsesDto = [];

            if (!horses.Any())
            {
                return Ok(new RetrieveArray<HorseRetrievingDto>());
            }
            else
            {
                foreach (var horse in horses)
                {
                    var horseDto = await MapHorse(horse);
                    horsesDto.Add(horseDto);
                }

                return Ok(new RetrieveArray<HorseRetrievingDto>(horsesDto.ToArray()));
            }
        }
        else
        {
            var horse = await db.Horses
                .SingleOrDefaultAsync(h => h.HorseId == horseId);
            
            if (horse is null)
            {
                return NotFound(new BadResponse(
                    Request.GetDisplayUrl(),
                    "Not found",
                    HttpStatusCode.Unauthorized,
                    new Collection<Error> { new("Horse not found", "Horse not exists") }));
            }

            HorseRetrievingDto horseDto = await MapHorse(horse);
            return Ok(horseDto);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(HorseRetrievingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HorseRetrievingDto>> CreateAsync([FromBody] HorseForCreationDto requestHorse)
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Unauthorized",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("Unauthorized", "User unauthorized") }));
        }

        Horse horse = new()
        {
            Name = requestHorse.Name,
            Description = requestHorse.Description,
            BirthDate = requestHorse.BirthDate,
            Sex = requestHorse.Sex,
            City = requestHorse.City,
            Region = requestHorse.Region,
            Country = requestHorse.Country,
            CreationDate = DateTime.UtcNow,
            LastUpdate = DateTime.UtcNow
        };

        //Add current user
        horse.HorseOwners.Add(new HorseOwner
        {
            Horse = horse,
            UserId = user.Id,
            RuleAccess = OwnerAccessRole.Creator.ToString()
        });

        if (requestHorse.HorseOwners is not null)
        {
            //Check the possibility of granting role to an object
            var check = requestHorse.HorseOwners.Where(hw => Enum.Parse<OwnerAccessRole>(hw.RuleAccess) >= OwnerAccessRole.Creator);

            if (check.Any())
            {
                return BadRequest(new BadResponse(
                    Request.GetDisplayUrl(),
                    "Role Access",
                    HttpStatusCode.BadRequest,
                    new Collection<Error> { new("Invalid Role", "Some roles access can not be greater or equal than your") }));
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
                    
                    if (!Enum.TryParse(horseOwner.RuleAccess, true, out OwnerAccessRole role))
                    {
                        return BadRequest(new BadResponse(
                            Request.GetDisplayUrl(),
                            "Role Access",
                            HttpStatusCode.BadRequest,
                            new Collection<Error> { new("Invalid Role", $"Role {horseOwner.RuleAccess} invalid") }));
                    }

                    switch (role)
                    {
                        case OwnerAccessRole.Read:
                            horseOwner.RuleAccess = OwnerAccessRole.Read.ToString();
                            break;
                        case OwnerAccessRole.Write:
                            horseOwner.RuleAccess = OwnerAccessRole.Write.ToString();
                            break;
                        case OwnerAccessRole.All:
                            horseOwner.RuleAccess = OwnerAccessRole.All.ToString();
                            break;
                        default:
                            return BadRequest(new BadResponse(
                                Request.GetDisplayUrl(),
                                "Role Access",
                                HttpStatusCode.BadRequest,
                                new Collection<Error> { new("Invalid Role", $"Role {role} not exists or does not apply to user") }));
                    }

                    horse.HorseOwners.Add(new HorseOwner
                    {
                        Horse = horse,
                        UserId = horseOwner.UserId,
                        RuleAccess = horseOwner.RuleAccess.ToString(),
                    });
                }
            }
            else
            {
                return BadRequest(new BadResponse(
                    Request.GetDisplayUrl(),
                    "Bad request",
                    HttpStatusCode.BadRequest,
                    new Collection<Error> { new("Users not found", "Some users not found") }));
            }
        }

        var entry = db.Horses.Add(horse);
        await db.SaveChangesAsync();

        return Created($"api/[controller]/horseId={horse.HorseId}", await MapHorse(entry.Entity));
    }

    [HttpPost]
    [Route("owners")]
    [ProducesResponseType(typeof(HorseRetrievingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HorseRetrievingDto>> UpdateHorseOwnersAsync([FromBody] RequestUpdateHorseOwners requestHorseOwners)
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Unauthorized",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("Unauthorized", "User unauthorized") }));
        }

        //Search existing horse owner
        HorseOwner? currentOwner = await db.HorseOwners.FirstOrDefaultAsync(
            hw => hw.User == user && hw.HorseId == requestHorseOwners.HorseId);

        if (currentOwner is null)
        {
            return BadRequest("You do not have access to this object");
        }

        OwnerAccessRole currentOwnerRole = Enum.Parse<OwnerAccessRole>(currentOwner.RuleAccess, true);

        //Check access to update
        if (currentOwnerRole < OwnerAccessRole.All)
        {
            return BadRequest("Access denied");
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

            if (!Enum.TryParse(reqOwner.RuleAccess, true, out OwnerAccessRole role))
            {
                return BadRequest(new BadResponse(
                    Request.GetDisplayUrl(),
                    "Role Access",
                    HttpStatusCode.BadRequest,
                    new Collection<Error> { new("Invalid Role", $"Role {reqOwner.RuleAccess} invalid") }));
            }

            if (role >= Enum.Parse<OwnerAccessRole>(currentOwner.RuleAccess, true))
            {
                return BadRequest(new BadResponse(
                    Request.GetDisplayUrl(),
                    "Role Access",
                    HttpStatusCode.BadRequest,
                    new Collection<Error> { new("Invalid Role", $"You can't add a role that is higher than yours") }));
            }

            newOwners.Add(new HorseOwner
            {
                UserId = reqOwner.UserId,
                HorseId = requestHorseOwners.HorseId,
                RuleAccess = reqOwner.RuleAccess
            });
        }

        newOwners.Add(currentOwner);

        await db.HorseOwners.AddRangeAsync(newOwners);
        await db.SaveChangesAsync();

        return await MapHorse(horse);
    }

    [HttpPost]
    [Route("add-owner")]
    [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OwnerDto>> AddOwnerAsync([FromBody] CreationHorseOwner requestOwner)
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Unauthorized",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("Unauthorized", "User unauthorized") }));
        }

        requestOwner.OwnerAccessRole ??= OwnerAccessRole.Read.ToString();

        if (!Enum.TryParse(requestOwner.OwnerAccessRole, true, out OwnerAccessRole role))
        {
            return BadRequest(new BadResponse(
               Request.GetDisplayUrl(),
               "Argument Exception",
               HttpStatusCode.BadRequest,
               new Collection<Error> { new("Rule Access", $"Impossible argument {requestOwner.OwnerAccessRole}") }));
        }

        HorseOwner? currentOwner = await db.HorseOwners.SingleOrDefaultAsync(
            o => o.User == user && o.HorseId == requestOwner.HorseId);

        if (currentOwner is null)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Access",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Not found", "Horse not exists or permission denied for it") }));
        }

        if (Enum.Parse<OwnerAccessRole>(currentOwner.RuleAccess, true) < OwnerAccessRole.All)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Access",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Permission denied", "Permission denied for the object") }));
        }

        User? newOwner = await db.Users
            .Include(u => u.HorseOwners)
            .SingleOrDefaultAsync(u => u.Id == requestOwner.UserId);

        if (newOwner is null)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Not Found",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Not Found", $"User (id: {requestOwner.UserId}) not found") }));
        }

        HorseOwner? existOwner = newOwner.HorseOwners.SingleOrDefault(o => o.HorseId == requestOwner.HorseId);

        if (existOwner is not null)
        {
            return BadRequest(new BadResponse(
               Request.GetDisplayUrl(),
               "Owner exists",
               HttpStatusCode.BadRequest,
               new Collection<Error> { new("Owner exists", $"Adding owner is already exists") }));
        }

        if (role >= Enum.Parse<OwnerAccessRole>(currentOwner.RuleAccess, true))
        {
            return BadRequest(new BadResponse(
               Request.GetDisplayUrl(),
               "Bad argument",
               HttpStatusCode.BadRequest,
               new Collection<Error> { new("Access Role", $"Owner role cannot be equal or above than yours") }));
        }

        db.HorseOwners.Add(new HorseOwner()
        {
            HorseId = requestOwner.HorseId,
            UserId = requestOwner.UserId,
            RuleAccess = requestOwner.OwnerAccessRole.ToString(),
        });

        await db.SaveChangesAsync();

        return Created(Request.GetDisplayUrl(), new OwnerDto()
        {
            Id = requestOwner.UserId,
            FirstName = newOwner.FirstName,
            LastName = newOwner.LastName,
            OwnerAccessRole = requestOwner.OwnerAccessRole.ToString()
        });
    }

    [HttpPost]
    [Route("change-owner-role")]
    public async Task<ActionResult<OwnerDto>> ChangeOwnerRoleAccessAsync(CreationHorseOwner requestOwner)
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Unauthorized",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("Unauthorized", "User unauthorized") }));
        }

        requestOwner.OwnerAccessRole ??= OwnerAccessRole.Read.ToString();

        if (!Enum.TryParse(requestOwner.OwnerAccessRole, true, out OwnerAccessRole role))
        {
            return BadRequest(new BadResponse(
               Request.GetDisplayUrl(),
               "Argument Exception",
               HttpStatusCode.BadRequest,
               new Collection<Error> { new("Rule Access", $"Impossible argument {requestOwner.OwnerAccessRole}") }));
        }

        if (user.Id == requestOwner.UserId)
        {
            return BadRequest(new BadResponse(
               Request.GetDisplayUrl(),
               "Argumet incorrect",
               HttpStatusCode.BadRequest,
               new Collection<Error> { new("Change yourself", $"You can't change yourself") }));
        }

        HorseOwner? currentOwner = await db.HorseOwners.SingleOrDefaultAsync(
            o => o.User == user && o.HorseId == requestOwner.HorseId);

        if (currentOwner is null)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Access",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Not found", "Horse not exists or permission denied for it") }));
        }

        if (Enum.Parse<OwnerAccessRole>(currentOwner.RuleAccess, true) < OwnerAccessRole.All)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Access",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Permission denied", "Permission denied for the object") }));
        }

        HorseOwner? owner = await db.HorseOwners.SingleOrDefaultAsync(
            o => o.UserId == requestOwner.UserId && o.HorseId == requestOwner.HorseId);

        if (owner is null)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Object not exists",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Horse owner not exists", "Horse owner not exists") }));
        }

        owner.RuleAccess = requestOwner.OwnerAccessRole;
        await db.SaveChangesAsync();

        var reference = db.Entry(owner).Reference(o => o.User);
        await reference.LoadAsync();

        return Ok(new OwnerDto()
        {
            Id = owner.UserId,
            FirstName = owner.User.FirstName,
            LastName = owner.User.LastName,
            OwnerAccessRole = owner.RuleAccess
        });
    }

    [HttpDelete]
    [Route("owners")]
    public async Task<ActionResult> DeleteOwnerAsync(long horseId, long userId)
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Unauthorized",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("Unauthorized", "User unauthorized") }));
        }

        if (user.Id == userId)
        {
            return BadRequest(new BadResponse(
               Request.GetDisplayUrl(),
               "Argumet incorrect",
               HttpStatusCode.BadRequest,
               new Collection<Error> { new("Delete yourself", $"You can't delete yourself") }));
        }

        HorseOwner? currentOwner = await db.HorseOwners.SingleOrDefaultAsync(
            o => o.User == user && o.HorseId == horseId);

        if (currentOwner is null)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Access",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Not found", "Horse not exists or permission denied for it") }));
        }

        if (Enum.Parse<OwnerAccessRole>(currentOwner.RuleAccess, true) < OwnerAccessRole.All)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Access",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Permission denied", "Permission denied for the object") }));
        }

        HorseOwner? owner = await db.HorseOwners.SingleOrDefaultAsync(
            o => o.UserId == userId && o.HorseId == horseId);

        if (owner is null)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Object not exists",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Horse owner not exists", "Horse owner not exists") }));
        }

        db.Remove(owner);
        await db.SaveChangesAsync();

        return Ok();
    }

    [HttpPut]
    [ProducesResponseType(typeof(HorseRetrievingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HorseRetrievingDto>> UpdateAsync([FromBody] HorseForUpdateDto horse)
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Unauthorized",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("Unauthorized", "User unauthorized") }));
        }

        Horse? entity = await db.Horses.Include(h => h.HorseOwners)
            .FirstOrDefaultAsync(h => h.HorseId == horse.HorseId);

        if (entity == null)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Bad Request",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Object non-existent", $"Horse (id: {horse.HorseId}) not found") }));
        }

        HorseOwner? owner = entity.HorseOwners.SingleOrDefault(o => o.User == user && o.HorseId == horse.HorseId);

        if (owner is null)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Bad Request",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Access denied", $"You do not have permissions for the current action") }));
        }

        if (Enum.Parse<OwnerAccessRole>(owner.RuleAccess, true) < OwnerAccessRole.Write)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Bad Request",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Access denied", $"You do not have permissions for the current action") }));
        }

        entity.Name = horse.Name;
        entity.BirthDate = horse.BirthDate;
        entity.Sex = horse.Sex;
        entity.City = horse.City;
        entity.Region = horse.Region;
        entity.Country = horse.Country;

        await db.SaveChangesAsync();

        return await MapHorse(entity);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAsync(long id)
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Unauthorized",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("Unauthorized", "User unauthorized") }));
        }

        HorseOwner? owner = db.HorseOwners.SingleOrDefault(o => o.User == user && o.HorseId == id);

        if (owner is null)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Bad Request",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Access denied", $"You do not have permissions for the current action") }));
        }

        if (Enum.Parse<OwnerAccessRole>(owner.RuleAccess, true) == OwnerAccessRole.Creator)
        {
            var referenceHorse = db.Entry(owner).Reference(o => o.Horse);
            await referenceHorse.LoadAsync();

            Horse? horse = referenceHorse.CurrentValue;

            if (horse is null)
            {
                return BadRequest(new BadResponse(
                    Request.GetDisplayUrl(),
                    "Bad Request",
                    HttpStatusCode.BadRequest,
                    new Collection<Error> { new("Horse is null", $"Horse not found") }));
            }

            db.Remove(horse);
        }
        else
        {
            db.Remove(owner);
        }

        await db.SaveChangesAsync();
        return Ok();
    }

    private async Task<HorseRetrievingDto> MapHorse(Horse horse)
    {
        HorseRetrievingDto horseDto = new()
        {
            HorseId = horse.HorseId,
            Name = horse.Name,
            Description = horse.Description,
            BirthDate = horse.BirthDate,
            Sex = horse.Sex,
            City = horse.City,
            Region = horse.Region,
            Country = horse.Country,
            CreationDate = horse.CreationDate,
        };

        horseDto.Users = new List<OwnerDto>();

        if (horse.HorseOwners != null)
        {
            foreach (var owner in horse.HorseOwners)
            {
                horseDto.Users.Add(new()
                {
                    Id = owner.UserId,
                    FirstName = owner.User.FirstName,
                    LastName = owner.User.LastName,
                    OwnerAccessRole = owner.RuleAccess
                });
            }
        }
        else
        {
            var ownersCollection = db.Entry(horse).Collection(h => h.HorseOwners);
            foreach (var owner in ownersCollection.CurrentValue!)
            {
                await db.Entry(owner).Reference(o => o.User).LoadAsync();

                horseDto.Users.Add(new()
                {
                    Id = owner.UserId,
                    FirstName = owner.User.FirstName,
                    LastName = owner.User.LastName,
                    OwnerAccessRole = owner.RuleAccess
                });
            }
        }

        CollectionEntry<Horse, Save> savesCollection = db.Entry(horse)
            .Collection(h => h.Saves);
        await savesCollection.LoadAsync();

        foreach (var save in savesCollection.CurrentValue!)
        {
            horseDto.Saves.Add(new()
            {
                SaveId = save.SaveId,
                Header = save.Header,
                Description = save.Description,
                Date = save.Date,
            });
        }

        return horseDto;
    }
}

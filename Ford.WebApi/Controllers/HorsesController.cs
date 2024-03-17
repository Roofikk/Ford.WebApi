using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Data;
using Microsoft.AspNetCore.Authorization;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Ford.WebApi.Dtos.Response;
using Ford.WebApi.Dtos.Horse;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net;
using Microsoft.AspNetCore.Identity;
using System.Security.AccessControl;

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
    public async Task<ActionResult> GetAsync(long? horseId, int below = 0, int above = 20)
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
                .Include(h => h.Users)
                .ThenInclude(o => o.User)
                .Where(h => h.Users.Any(o => o.UserId == user.Id))
                .Skip(below)
                .Take(above - below)
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

        //Check the possibility of granting role to an object
        var check = requestHorse.Users
            .Where(u => u.UserId != user.Id && Enum.Parse<OwnerAccessRole>(u.RuleAccess) >= OwnerAccessRole.Creator);

        if (check.Any())
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Role Access",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Invalid Role", "Some roles access can not be greater or equal than your") }));
        }

        //Find exist user in DB
        IEnumerable<User> containsUsers = db.Users
            .Where(u => requestHorse.Users
            .Select(o => o.UserId)
            .Contains(u.Id));

        if (containsUsers.Count() == requestHorse.Users.Count())
        {
            foreach (var reqUser in requestHorse.Users)
            {
                // add yourself
                if (reqUser.UserId == user.Id)
                {
                    horse.Users.Add(new()
                    {
                        UserId = reqUser.UserId,
                        RuleAccess = OwnerAccessRole.Creator.ToString(),
                        IsOwner = reqUser.IsOwner,
                    });

                    continue;
                }
                    
                if (!Enum.TryParse(reqUser.RuleAccess, true, out OwnerAccessRole role))
                {
                    return BadRequest(new BadResponse(
                        Request.GetDisplayUrl(),
                        "Role Access",
                        HttpStatusCode.BadRequest,
                        new Collection<Error> { new("Invalid Role", $"Role {reqUser.RuleAccess} invalid") }));
                }

                horse.Users.Add(new()
                {
                    UserId = reqUser.UserId,
                    RuleAccess = reqUser.RuleAccess.ToString(),
                    IsOwner = reqUser.IsOwner,
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

        // add yourserl
        if (!horse.Users.Any(u => u.UserId == user.Id))
        {
            horse.Users.Add(new()
            {
                UserId = user.Id,
                RuleAccess = OwnerAccessRole.Creator.ToString(),
                IsOwner = false,
            });
        }

        var findOwner = horse.Users.SingleOrDefault(u => u.IsOwner);

        if (findOwner == null)
        {
            horse.OwnerName = requestHorse.OwnerName;
            horse.OwnerPhoneNumber = requestHorse.OwnerPhoneNumber;
        }

        // push saves
        foreach (var save in requestHorse.Saves)
        {
            horse.Saves.Add(new()
            {
                Header = save.Header,
                Description = save.Description,
                Date = save.Date,
                CreationDate = save.CreationDate ?? DateTime.UtcNow,
                LastUpdate = save.LastUpdate ?? DateTime.UtcNow,
                User = user,
            });
        }

        var entry = db.Horses.Add(horse);
        await db.SaveChangesAsync();

        return Created($"api/[controller]/horseId={horse.HorseId}", await MapHorse(entry.Entity));
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

        Horse? entity = await db.Horses.Include(h => h.Users)
            .FirstOrDefaultAsync(h => h.HorseId == horse.HorseId);

        if (entity == null)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Bad Request",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Object non-existent", $"Horse (id: {horse.HorseId}) not found") }));
        }

        UserHorse? owner = entity.Users.SingleOrDefault(o => o.User == user && o.HorseId == horse.HorseId);

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

        UserHorse? owner = db.HorseOwners.SingleOrDefault(o => o.User == user && o.HorseId == id);

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
            OwnerName = horse.OwnerName,
            OwnerPhoneNumber = horse.OwnerPhoneNumber,
            CreationDate = horse.CreationDate,
            LastUpdate = horse.LastUpdate,
        };

        horseDto.Users = new List<HorseUserDto>();

        if (horse.Users != null)
        {
            foreach (var owner in horse.Users)
            {
                if (owner.User == null)
                {
                    await db.Entry(owner).Reference(o => o.User).LoadAsync();
                }

                horseDto.Users.Add(new()
                {
                    Id = owner.UserId,
                    FirstName = owner.User!.FirstName,
                    LastName = owner.User.LastName,
                    PhoneNumber = owner.User.PhoneNumber,
                    IsOwner = owner.IsOwner,
                    AccessRole = owner.RuleAccess
                });
            }
        }
        else
        {
            var ownersCollection = db.Entry(horse).Collection(h => h.Users);
            foreach (var owner in ownersCollection.CurrentValue!)
            {
                await db.Entry(owner).Reference(o => o.User).LoadAsync();

                horseDto.Users.Add(new()
                {
                    Id = owner.UserId,
                    FirstName = owner.User.FirstName,
                    LastName = owner.User.LastName,
                    PhoneNumber = owner.User.PhoneNumber,
                    IsOwner = owner.IsOwner,
                    AccessRole = owner.RuleAccess
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
                HorseId = horse.HorseId,
                UserId = save.UserId,
                Header = save.Header,
                Description = save.Description,
                Date = save.Date,
                CreationDate = save.CreationDate,
                LastUpdate = save.LastUpdate,
            });
        }

        return horseDto;
    }
}

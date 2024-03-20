using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Data;
using Microsoft.AspNetCore.Authorization;
using System.Collections.ObjectModel;
using Ford.WebApi.Dtos.Response;
using Ford.WebApi.Dtos.Horse;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net;
using Ford.WebApi.Filters;
using Ford.WebApi.Services;

namespace Ford.WebApi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(UserFilter))]
public class HorsesController : ControllerBase
{
    private readonly FordContext db;
    private readonly ISaveRepository _saveService;
    private readonly IUserHorseRepository _horseUserService;
    private User? user = null;

    public HorsesController(FordContext db, ISaveRepository saveRepository, IUserHorseRepository userHorseRepository)
    {
        this.db = db;
        _saveService = saveRepository;
        _horseUserService = userHorseRepository;
    }

    [HttpGet()]
    [ProducesResponseType(typeof(RetrieveArray<HorseRetrievingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HorseRetrievingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetAsync(long? horseId, int below = 0, int amount = 20,
        string orderByDate = "desc", string orderByName = "false")
    {
        user ??= (User)HttpContext.Items["user"]!;

        if (horseId == null)
        {
            var queryableHorses = db.Horses.Where(h => h.Users.Any(o => o.UserId == user.Id));

            switch (orderByDate)
            {
                case "true":
                    queryableHorses = queryableHorses.OrderBy(o => o.LastUpdate);
                    break;
                case "desc":
                    queryableHorses = queryableHorses.OrderByDescending(o => o.LastUpdate);
                    break;
            }

            switch (orderByName)
            {
                case "true":
                    queryableHorses = queryableHorses.OrderBy(o => o.Name);
                    break;
                case "desc":
                    queryableHorses = queryableHorses.OrderByDescending(o => o.Name);
                    break;
            }

            IEnumerable<Horse> horses = queryableHorses
                .Skip(below)
                .Take(amount)
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
        user ??= (User)HttpContext.Items["user"]!;

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
            .Where(u => u.UserId != user.Id && Enum.Parse<UserAccessRole>(u.AccessRole) >= UserAccessRole.Creator);

        if (check.Any())
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Role Access",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Invalid Role", "Some roles access can not be greater or equal than your") }));
        }

        var result = _horseUserService.Create(user, horse, requestHorse.Users);

        if (!result.Success)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Add users error",
                HttpStatusCode.BadRequest,
                [new("Exception", result.ErrorMessage!)]));
        }

        horse = result.Result!;

        var findOwner = horse.Users.SingleOrDefault(u => u.IsOwner);

        if (findOwner == null)
        {
            horse.OwnerName = requestHorse.OwnerName;
            horse.OwnerPhoneNumber = requestHorse.OwnerPhoneNumber;
        }

        var creaeteSaveResult = _saveService.Create(horse, requestHorse.Saves, user.Id);

        if (!creaeteSaveResult.Success)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Add saves error",
                HttpStatusCode.BadRequest,
                [new("Exception", result.ErrorMessage!)]));
        }

        horse = result.Result!;

        var entry = db.Horses.Add(horse);
        await db.SaveChangesAsync();

        return Created($"api/[controller]/horseId={horse.HorseId}", await MapHorse(entry.Entity));
    }

    [HttpPut]
    [ProducesResponseType(typeof(HorseRetrievingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
    [TypeFilter(typeof(AccessRoleFilter), Arguments = [UserAccessRole.Write])]
    public async Task<ActionResult<HorseRetrievingDto>> UpdateAsync([FromBody] RequestUpdateHorseDto requestHorse)
    {
        Horse? entity = await db.Horses.Include(h => h.Users)
            .FirstOrDefaultAsync(h => h.HorseId == requestHorse.HorseId);

        if (entity == null)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Bad Request",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Object non-existent", $"Horse (id: {requestHorse.HorseId}) not found") }));
        }

        entity.Name = requestHorse.Name;
        entity.BirthDate = requestHorse.BirthDate;
        entity.Sex = requestHorse.Sex;
        entity.City = requestHorse.City;
        entity.Region = requestHorse.Region;
        entity.Country = requestHorse.Country;

        var result = await _horseUserService.UpdateAsync(user!.Id, entity, requestHorse.Users);

        if (!result.Success)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Update users error",
                HttpStatusCode.BadRequest,
                [new("Exception", result.ErrorMessage!)]));
        }

        entity = result.Result!;
        db.Entry(entity).State = EntityState.Modified;

        await db.SaveChangesAsync();
        return await MapHorse(entity);
    }

    [HttpDelete]
    [TypeFilter(typeof(AccessRoleFilter), Arguments = [UserAccessRole.Read])]
    public async Task<IActionResult> DeleteAsync(long horseId)
    {
        user ??= (User)HttpContext.Items["user"]!;
        var horseUser = (UserHorse)HttpContext.Items["horseUser"]!;

        if (Enum.Parse<UserAccessRole>(horseUser.AccessRole, true) == UserAccessRole.Creator)
        {
            Horse horse = await db.Horses.SingleAsync(h => h.HorseId == horseId);
            db.Remove(horse);
        }
        else
        {
            db.Remove(horseUser);
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

        if (horse.Users.Count > 0)
        {
            foreach (var owner in horse.Users)
            {
                if (owner.User == null)
                {
                    await db.Entry(owner).Reference(o => o.User).LoadAsync();
                }

                horseDto.Users.Add(new()
                {
                    UserId = owner.UserId,
                    FirstName = owner.User!.FirstName,
                    LastName = owner.User.LastName,
                    PhoneNumber = owner.User.PhoneNumber,
                    IsOwner = owner.IsOwner,
                    AccessRole = owner.AccessRole
                });
            }
        }
        else
        {
            var usersCollection = db.Entry(horse).Collection(h => h.Users);
            await usersCollection.LoadAsync();
            foreach (var user in usersCollection.CurrentValue!)
            {
                await db.Entry(user).Reference(o => o.User).LoadAsync();

                if (this.user!.Id == user.UserId)
                {
                    horseDto.Self = new()
                    {
                        UserId = user.UserId,
                        FirstName = user.User.FirstName,
                        LastName = user.User.LastName,
                        PhoneNumber = user.User.PhoneNumber,
                        AccessRole = user.AccessRole,
                        IsOwner = user.IsOwner,
                    };
                    continue;
                }

                horseDto.Users.Add(new()
                {
                    UserId = user.UserId,
                    FirstName = user.User.FirstName,
                    LastName = user.User.LastName,
                    PhoneNumber = user.User.PhoneNumber,
                    IsOwner = user.IsOwner,
                    AccessRole = user.AccessRole
                });
            }
        }

        var saves = await _saveService.GetAsync(horse.HorseId, user!.Id, 0, 20);
        horseDto.Saves = saves;

        return horseDto;
    }
}

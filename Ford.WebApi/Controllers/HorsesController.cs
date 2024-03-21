﻿using Microsoft.AspNetCore.Mvc;
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
    private readonly FordContext _context;
    private readonly ISaveRepository _saveService;
    private readonly IUserHorseRepository _horseUserService;
    private User? _user = null;

    public HorsesController(FordContext db, ISaveRepository saveRepository, IUserHorseRepository userHorseRepository)
    {
        this._context = db;
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
        _user ??= (User)HttpContext.Items["user"]!;

        if (horseId == null)
        {
            var queryableHorses = _context.Horses.Where(h => h.Users.Any(o => o.UserId == _user.Id));

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
            var horse = await _context.Horses
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
        _user ??= (User)HttpContext.Items["user"]!;

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
            .Where(u => u.UserId != _user.Id && Enum.Parse<UserAccessRole>(u.AccessRole) >= UserAccessRole.Creator);

        if (check.Any())
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Role Access",
                HttpStatusCode.BadRequest,
                new Collection<Error> { new("Invalid Role", "Some roles access can not be greater or equal than your") }));
        }

        var result = _horseUserService.Create(_user, horse, requestHorse.Users);

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

        var creaeteSaveResult = _saveService.Create(horse, requestHorse.Saves, _user.Id);

        if (!creaeteSaveResult.Success)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Add saves error",
                HttpStatusCode.BadRequest,
                [new("Exception", result.ErrorMessage!)]));
        }

        horse = result.Result!;

        var entry = _context.Horses.Add(horse);
        await _context.SaveChangesAsync();

        return Created($"api/[controller]/horseId={horse.HorseId}", await MapHorse(entry.Entity));
    }

    [HttpPut]
    [ProducesResponseType(typeof(HorseRetrievingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
    [TypeFilter(typeof(AccessRoleFilter), Arguments = [UserAccessRole.Write])]
    public async Task<ActionResult<HorseRetrievingDto>> UpdateAsync([FromBody] RequestUpdateHorseDto requestHorse)
    {
        _user ??= (User)HttpContext.Items["user"]!;

        Horse? entity = await _context.Horses.Include(h => h.Users)
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

        var result = await _horseUserService.UpdateAsync(_user.Id, entity, requestHorse.Users);

        if (!result.Success)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Update users error",
                HttpStatusCode.BadRequest,
                [new("Exception", result.ErrorMessage!)]));
        }

        entity = result.Result!;
        _context.Entry(entity).State = EntityState.Modified;

        if (entity.Users.SingleOrDefault(u => u.IsOwner) == null)
        {
            entity.OwnerName = requestHorse.OwnerName;
            entity.OwnerPhoneNumber = requestHorse.OwnerPhoneNumber;
        }
        else
        {
            entity.OwnerName = null;
            entity.OwnerPhoneNumber = null;
        }

        await _context.SaveChangesAsync();
        return await MapHorse(entity);
    }

    [HttpDelete]
    [TypeFilter(typeof(AccessRoleFilter), Arguments = [UserAccessRole.Read])]
    public async Task<IActionResult> DeleteAsync(long horseId)
    {
        _user ??= (User)HttpContext.Items["user"]!;
        var horseUser = (UserHorse)HttpContext.Items["horseUser"]!;

        if (Enum.Parse<UserAccessRole>(horseUser.AccessRole, true) == UserAccessRole.Creator)
        {
            Horse horse = await _context.Horses.SingleAsync(h => h.HorseId == horseId);
            _context.Remove(horse);
        }
        else
        {
            _context.Remove(horseUser);
        }

        await _context.SaveChangesAsync();
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

        var usersCollection = _context.Entry(horse).Collection(h => h.Users);
        await usersCollection.LoadAsync();
        foreach (var user in usersCollection.CurrentValue!)
        {
            await _context.Entry(user).Reference(o => o.User).LoadAsync();

            if (_user!.Id == user.UserId)
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

        var saves = await _saveService.GetAsync(horse.HorseId, _user!.Id, 0, 20);
        horseDto.Saves = saves;

        return horseDto;
    }
}

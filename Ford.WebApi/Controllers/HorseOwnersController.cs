using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Dtos.Request;
using Ford.WebApi.Dtos.Response;
using Ford.WebApi.Filters;
using Ford.WebApi.Models.Horse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Net;

namespace Ford.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HorseOwnersController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly FordContext db;

        public HorseOwnersController(UserManager<User> userManager, FordContext context)
        {
            this.userManager = userManager;
            db = context;
        }

        [HttpPost]
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
            UserHorse? currentOwner = await db.HorseUsers.FirstOrDefaultAsync(
                hw => hw.User == user && hw.HorseId == requestHorseOwners.HorseId);

            if (currentOwner is null)
            {
                return BadRequest("You do not have access to this object");
            }

            UserAccessRole currentOwnerRole = Enum.Parse<UserAccessRole>(currentOwner.RuleAccess, true);

            //Check access to update
            if (currentOwnerRole < UserAccessRole.All)
            {
                return BadRequest("Access denied");
            }

            Collection<UserHorse> newOwners = new();

            foreach (var reqOwner in requestHorseOwners.HorseOwners)
            {
                if (reqOwner.UserId == user.Id)
                    continue;

                if (!Enum.TryParse(reqOwner.RuleAccess, true, out UserAccessRole role))
                {
                    return BadRequest(new BadResponse(
                        Request.GetDisplayUrl(),
                        "Role Access",
                        HttpStatusCode.BadRequest,
                        new Collection<Error> { new("Invalid Role", $"Role {reqOwner.RuleAccess} invalid") }));
                }

                if (role >= Enum.Parse<UserAccessRole>(currentOwner.RuleAccess, true))
                {
                    return BadRequest(new BadResponse(
                        Request.GetDisplayUrl(),
                        "Role Access",
                        HttpStatusCode.BadRequest,
                        new Collection<Error> { new("Invalid Role", $"You can't add a role that is higher than yours") }));
                }

                newOwners.Add(new UserHorse
                {
                    UserId = reqOwner.UserId,
                    HorseId = requestHorseOwners.HorseId,
                    RuleAccess = reqOwner.RuleAccess
                });
            }

            newOwners.Add(currentOwner);

            await db.HorseUsers.AddRangeAsync(newOwners);
            await db.SaveChangesAsync();

            return RedirectToAction("GetAsync", "HorsesController", requestHorseOwners.HorseId);
        }

        [HttpPost]
        [Route("add-owner")]
        [ProducesResponseType(typeof(HorseUserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<HorseUserDto>> AddOwnerAsync([FromBody] CreationHorseOwner requestOwner)
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

            requestOwner.OwnerAccessRole ??= UserAccessRole.Read.ToString();

            if (!Enum.TryParse(requestOwner.OwnerAccessRole, true, out UserAccessRole role))
            {
                return BadRequest(new BadResponse(
                   Request.GetDisplayUrl(),
                   "Argument Exception",
                   HttpStatusCode.BadRequest,
                   new Collection<Error> { new("Rule Access", $"Impossible argument {requestOwner.OwnerAccessRole}") }));
            }

            UserHorse? currentOwner = await db.HorseUsers.SingleOrDefaultAsync(
                o => o.User == user && o.HorseId == requestOwner.HorseId);

            if (currentOwner is null)
            {
                return BadRequest(new BadResponse(
                    Request.GetDisplayUrl(),
                    "Access",
                    HttpStatusCode.BadRequest,
                    new Collection<Error> { new("Not found", "Horse not exists or permission denied for it") }));
            }

            if (Enum.Parse<UserAccessRole>(currentOwner.RuleAccess, true) < UserAccessRole.All)
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

            UserHorse? existOwner = newOwner.HorseOwners.SingleOrDefault(o => o.HorseId == requestOwner.HorseId);

            if (existOwner is not null)
            {
                return BadRequest(new BadResponse(
                   Request.GetDisplayUrl(),
                   "Owner exists",
                   HttpStatusCode.BadRequest,
                   new Collection<Error> { new("Owner exists", $"Adding owner is already exists") }));
            }

            if (role >= Enum.Parse<UserAccessRole>(currentOwner.RuleAccess, true))
            {
                return BadRequest(new BadResponse(
                   Request.GetDisplayUrl(),
                   "Bad argument",
                   HttpStatusCode.BadRequest,
                   new Collection<Error> { new("Access Role", $"Owner role cannot be equal or above than yours") }));
            }

            db.HorseUsers.Add(new UserHorse()
            {
                HorseId = requestOwner.HorseId,
                UserId = requestOwner.UserId,
                RuleAccess = requestOwner.OwnerAccessRole.ToString(),
            });

            await db.SaveChangesAsync();

            return Created(Request.GetDisplayUrl(), new HorseUserDto()
            {
                UserId = requestOwner.UserId,
                FirstName = newOwner.FirstName,
                LastName = newOwner.LastName,
                PhoneNumber = newOwner.PhoneNumber,
                IsOwner = false,
                AccessRole = requestOwner.OwnerAccessRole.ToString()
            });
        }

        [HttpPost]
        [Route("change-owner-role")]
        [ProducesResponseType(typeof(HorseUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<HorseUserDto>> ChangeOwnerRoleAccessAsync(CreationHorseOwner requestOwner)
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

            requestOwner.OwnerAccessRole ??= UserAccessRole.Read.ToString();

            if (!Enum.TryParse(requestOwner.OwnerAccessRole, true, out UserAccessRole role))
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

            UserHorse? currentOwner = await db.HorseUsers.SingleOrDefaultAsync(
                o => o.User == user && o.HorseId == requestOwner.HorseId);

            if (currentOwner is null)
            {
                return BadRequest(new BadResponse(
                    Request.GetDisplayUrl(),
                    "Access",
                    HttpStatusCode.BadRequest,
                    new Collection<Error> { new("Not found", "Horse not exists or permission denied for it") }));
            }

            if (Enum.Parse<UserAccessRole>(currentOwner.RuleAccess, true) < UserAccessRole.All)
            {
                return BadRequest(new BadResponse(
                    Request.GetDisplayUrl(),
                    "Access",
                    HttpStatusCode.BadRequest,
                    new Collection<Error> { new("Permission denied", "Permission denied for the object") }));
            }

            UserHorse? owner = await db.HorseUsers.SingleOrDefaultAsync(
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

            return Ok(new HorseUserDto()
            {
                UserId = owner.UserId,
                FirstName = owner.User.FirstName,
                LastName = owner.User.LastName,
                PhoneNumber = owner.User.PhoneNumber,
                IsOwner = owner.IsOwner,
                AccessRole = owner.RuleAccess
            });
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
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

            UserHorse? currentOwner = await db.HorseUsers.SingleOrDefaultAsync(
                o => o.User == user && o.HorseId == horseId);

            if (currentOwner is null)
            {
                return BadRequest(new BadResponse(
                    Request.GetDisplayUrl(),
                    "Access",
                    HttpStatusCode.BadRequest,
                    new Collection<Error> { new("Not found", "Horse not exists or permission denied for it") }));
            }

            if (Enum.Parse<UserAccessRole>(currentOwner.RuleAccess, true) < UserAccessRole.All)
            {
                return BadRequest(new BadResponse(
                    Request.GetDisplayUrl(),
                    "Access",
                    HttpStatusCode.BadRequest,
                    new Collection<Error> { new("Permission denied", "Permission denied for the object") }));
            }

            UserHorse? owner = await db.HorseUsers.SingleOrDefaultAsync(
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
    }
}

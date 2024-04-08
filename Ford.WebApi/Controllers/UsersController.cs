using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly FordContext _context;
    private readonly UserManager<User> _userManager;
#pragma warning disable CS0169 // The field 'UsersController._roleManager' is never used
    private readonly RoleManager<IdentityRole<long>> _roleManager;
#pragma warning restore CS0169 // The field 'UsersController._roleManager' is never used

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public UsersController(FordContext context, UserManager<User> userManager)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _context = context;
        this._userManager = userManager;
    }

    [HttpGet()]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<UserGettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserGettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long? userId)
    {
        if (userId != null)
        {
            User? user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                return NotFound(user);
            }
            else
            {
                var userDto = (MinimalUserDto)MapUser(user);
                return Ok(userDto);
            }
        }
        else
        {
            List<User>? users = await _context.Users.ToListAsync();
            List<MinimalUserDto> usersDto = [];

            foreach (var user in users)
            {
                usersDto.Add(MapUser(user));
            }

            return Ok(usersDto);
        }
    }

    //[HttpPost]
    //[Authorize]
    //public async Task<IActionResult> Create([FromBody] UserRegister requestUser, string secretKey, string role = "Member")
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        return BadRequest();
    //    }

    //    var user = new User
    //    {
    //        UserName = requestUser.Login,
    //        Email = requestUser.Email,
    //        FirstName = requestUser.FirstName,
    //        LastName = requestUser.LastName,
    //        BirthDate = requestUser.BirthDate,
    //        CreationDate = DateTime.UtcNow,
    //        LastUpdate = DateTime.UtcNow
    //    };



    //    return Ok();
    //}

    [HttpGet("search")]
    [Authorize]
    public async Task<ActionResult<MinimalUserDto>> FindUser([Required] string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);

        if (user == null)
        {
            return NotFound();
        }
        else
        {
            var userDto = (MinimalUserDto)MapUser(user);
            return Ok(userDto);
        }
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);

        if (user != null)
        {
            _context.Remove(user);
            await _context.SaveChangesAsync();
            return Ok();
        }
        else
        {
            return NotFound();
        }
    }

    private UserGettingDto MapUser(User user)
    {
        var userDto = new UserGettingDto()
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BirthDate = user.BirthDate,
            City = user.City,
            Region = user.Region,
            Country = user.Country,
            CreationDate = user.CreationDate,
            LastUpdatedDate = user.LastUpdate,
            PhoneNumber = user.PhoneNumber,
        };

        return userDto;
    }
}
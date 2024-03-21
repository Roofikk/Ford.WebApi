using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly FordContext db;
    private readonly UserManager<User> userManager;

    public UsersController(FordContext context, UserManager<User> userManager)
    {
        db = context;
        this.userManager = userManager;
    }

    [HttpGet()]
    [ProducesResponseType(typeof(IEnumerable<UserGettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserGettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long? id)
    {
        if (id != null)
        {
            User? user = await db.Users.SingleOrDefaultAsync(u => u.Id == id);

            if (user is null)
            {
                return NotFound(user);
            }
            else
            {
                var userDto = MapUser(user);
                return Ok(userDto);
            }
        }
        else
        {
            List<User>? users = await db.Users.ToListAsync();
            List<UserGettingDto> usersDto = new List<UserGettingDto>();

            foreach (var user in users)
            {
                usersDto.Add(MapUser(user));
            }

            return Ok(usersDto);
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<UserGettingDto>> FindUser([Required] string userName)
    {
        var user = await userManager.FindByNameAsync(userName);

        if (user == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(MapUser(user));
        }
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id);

        if (user != null)
        {
            db.Remove(user);
            await db.SaveChangesAsync();
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
            Login = user.UserName!,
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
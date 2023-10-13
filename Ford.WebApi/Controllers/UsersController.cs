using Ford.DataContext.Sqlite;
using Ford.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly FordContext db;

    public UsersController(FordContext db)
    {
        this.db = db;
    }

    //[Authorize]
    //Need role admin to access thit method
    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            User? user = db.Users.FirstOrDefault(u => u.UserId == id);
            if (user is null)
            {
                return BadRequest("User not found");
            }
            else
            {
                return Ok(user);
            }
        }
        else
        {
            return BadRequest("Id can not be null");
        }
    }
}
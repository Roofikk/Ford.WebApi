using Ford.DataContext.Sqlite;
using Ford.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthorizationController : ControllerBase
{
    private readonly FordContext db;

    public AuthorizationController(FordContext db)
    {
        this.db = db;
    }

    [HttpPost]
    public IActionResult Registration([FromBody] User user)
    {
        if (user is null)
        {
            return BadRequest("User is null");
        }

        if (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest("Login or password can not be empty");
        }

         User? existingUser = db.Users.FirstOrDefault(u => u.Login == user.Login);

        if (existingUser is not null)
        {
            return Conflict($"User with {user.Login} login is existing");
        }

        db.Users.Add(user);
        db.SaveChanges();

        return Ok();
    }
}

using Ford.DataContext.Sqlite;
using Ford.Models;
using Ford.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserRepository db;

    public UsersController(IUserRepository db)
    {
        this.db = db;
    }

    [HttpGet()]
    public async Task<IActionResult> Get(string? id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            User? user = await db.RetrieveAsync(id);

            if (user is null)
            {
                return NotFound();
            }
            else
            {
                return Ok(user);
            }
        }
        else
        {
            return Ok(await db.RetrieveAllAsync());
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody]User user)
    {
        if (await db.IsExist(user.UserId, user.Login))
        {
            return BadRequest("User already exists");
        }
        
        User? created = await db.CreateAsync(user);
        await db.Save();
        return Created("", created);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody]User user)
    {
        if (user.UserId is null)
        {
            return BadRequest("User id can not be null");
        }

        if (await db.IsExist(user.UserId))
        {
            User? updated = await db.UpdateAsync(user);
            await db.Save();
            return Ok(updated);
        }
        else
        {
            return NotFound(user);
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        if (await db.IsExist(id))
        {
            bool success = await db.DeleteAsync(id);

            if (success)
            {
                await db.Save();
                return Ok();
            }
            else
            {
                return BadRequest($"Failed to delete user by id: {id}");
            }
        }
        else
        {
            return NotFound($"User is not found by id: {id}");
        }
    }
}
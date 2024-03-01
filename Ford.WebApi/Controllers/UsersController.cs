using AutoMapper;
using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IMapper mapper;
    private readonly FordContext db;

    public UsersController(FordContext context, IMapper mapper)
    {
        db = context;
        this.mapper = mapper;
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
                var mappingUser = mapper.Map<UserGettingDto>(user);
                return Ok(mappingUser);
            }
        }
        else
        {
            List<User>? users = await db.Users.ToListAsync();
            return Ok(mapper.Map<IEnumerable<UserGettingDto>>(users));
        }
    }

    //[HttpPost]
    //[ProducesResponseType(StatusCodes.Status201Created)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<ActionResult<UserGettingDto>> Create([FromBody] UserCreationDto user)
    //{
    //    User sourceUser = mapper.Map<User>(user);
    //    var existUser = db.Users.SingleOrDefaultAsync(u => u.Id == user.)

    //    if (await db.IsExistAsync(sourceUser))
    //    {
    //        return BadRequest("User already exists");
    //    }
        
    //    User? created = await db.CreateAsync(sourceUser);
        
    //    if (created is null)
    //    {
    //        return BadRequest();
    //    }

    //    await db.SaveAsync();

    //    UserGettingDto responseUser = mapper.Map<UserGettingDto>(created);
    //    return CreatedAtAction(nameof(Get), new { id = created.Id }, responseUser);
    //}

    //[HttpPut]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //public async Task<ActionResult<UserGettingDto>> Update([FromBody]UserForUpdateDto user)
    //{
    //    User sourceUser = mapper.Map<User>(user);

    //    User? updated = await db.UpdateAsync(sourceUser);

    //    if (updated is null)
    //    {
    //        return NotFound(user);
    //    }

    //    await db.SaveAsync();
    //    UserGettingDto response = mapper.Map<UserGettingDto>(updated);
    //    return Ok(response);
    //}

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
}
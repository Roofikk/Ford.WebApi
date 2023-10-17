using AutoMapper;
using Ford.Models;
using Ford.WebApi.Dtos.User;
using Ford.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IRepository<User, string> db;
    private readonly IMapper mapper;

    public UsersController(IRepository<User, string> db, IMapper mapper)
    {
        this.db = db;
        this.mapper = mapper;
    }

    [HttpGet()]
    [ProducesResponseType(typeof(IEnumerable<UserGettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserGettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string? id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            User? user = await db.RetrieveAsync(id);

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
            IEnumerable<User> users = await db.RetrieveAllAsync();
            return Ok(mapper.Map<IEnumerable<UserGettingDto>>(users));
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserGettingDto>> Create([FromBody]UserCreationDto user)
    {
        User sourceUser = mapper.Map<User>(user);

        if (await db.IsExistAsync(sourceUser))
        {
            return BadRequest("User already exists");
        }
        
        User? created = await db.CreateAsync(sourceUser);
        await db.SaveAsync();

        UserGettingDto responseUser = mapper.Map<UserGettingDto>(created);
        return CreatedAtAction(nameof(Get), new { id = created.UserId }, responseUser);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserGettingDto>> Update([FromBody]UserForUpdateDto user)
    {
        User sourceUser = mapper.Map<User>(user);

        if (sourceUser.UserId is null)
        {
            return BadRequest("User id can not be null");
        }

        User? updated = await db.UpdateAsync(sourceUser);

        if (updated is null)
        {
            return NotFound(user);
        }

        await db.SaveAsync();
        UserGettingDto response = mapper.Map<UserGettingDto>(updated);
        return Ok(response);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        if (await db.IsExistAsync(id))
        {
            bool success = await db.DeleteAsync(id);

            if (success)
            {
                await db.SaveAsync();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
        else
        {
            return NotFound();
        }
    }
}
using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ford.WebApi.Dtos.Request;
using System.ComponentModel.DataAnnotations;
using Ford.WebApi.Services;
using Ford.WebApi.Filters;

namespace Ford.WebApi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(UserFilter))]
public class SavesController : ControllerBase
{
    private readonly FordContext db;
    private readonly ISaveRepository _saveService;
    private User? _user;

    public SavesController(FordContext db, ISaveRepository saveRepository)
    {
        this.db = db;
        _saveService = saveRepository;
    }

    // GET: api/<SavesController>/{horseId}
    [HttpGet()]
    [ProducesResponseType(typeof(FullSaveDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<SaveDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get([Required] long horseId, long? saveId, int below = 0, int amount = 20)
    {
        _user ??= (User)HttpContext.Items["user"]!;

        if (saveId == null)
        {
            var saves = await _saveService.GetAsync(horseId, _user.Id, below, amount);
            return Ok(saves);
        }
        else
        {
            var save = await _saveService.GetAsync(horseId, saveId.Value, _user.Id);
            return Ok(save);
        }
    }

    // POST api/<SavesController>/{horseId}
    // Create
    [HttpPost()]
    [TypeFilter(typeof(AccessRoleFilter), Arguments = [UserAccessRole.Write])]
    public async Task<ActionResult<SaveDto>> Create([FromBody] SaveCreatingDto requestSave)
    {
        // get authorize user
        _user ??= (User)HttpContext.Items["user"]!;
        var saveDto = await _saveService.CreateAsync(requestSave, _user.Id);

        if (saveDto == null)
        {
            return BadRequest();
        }

        return Created($"api/[controller]?horseId={requestSave.HorseId}&saveId={saveDto.SaveId}", saveDto);
    }

    // PUT api/<SavesController>/5
    // Update
    [HttpPut()]
    [TypeFilter(typeof(AccessRoleFilter), Arguments = [UserAccessRole.Write])]
    public async Task<ActionResult<SaveDto>> Update([FromBody] RequestUpdateSaveDto requestSave)
    {
        _user ??= (User)HttpContext.Items["user"]!;

        Save? save = await db.Saves.SingleOrDefaultAsync(s => s.SaveId == requestSave.SaveId);

        if (save is null)
        {
            return NotFound();
        }

        var saveDto = await _saveService.UpdateAsync(requestSave, save, _user.Id);

        if (saveDto == null)
        {
            return BadRequest();
        }

        return saveDto;
    }

    // DELETE api/[controller]?saveId=5
    [HttpDelete()]
    [TypeFilter(typeof(AccessRoleFilter), Arguments = [UserAccessRole.Write])]
    public async Task<IActionResult> Delete([Required] long saveId)
    {
        Save? save = await db.Saves.SingleOrDefaultAsync(s => s.SaveId == saveId);

        if (save is null)
        {
            return NotFound();
        }

        var result = await _saveService.DeleteAsync(save);
        if (result)
        {
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }
}

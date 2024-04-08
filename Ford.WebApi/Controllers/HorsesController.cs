using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Dtos.Request;
using Ford.WebApi.Dtos.Response;
using Ford.WebApi.Filters;
using Ford.WebApi.Services;
using Ford.WebApi.Services.HorseService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Ford.WebApi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(UserFilter))]
public class HorsesController : ControllerBase
{
    private readonly IHorseRepository _horseRepository;
    private readonly ISaveRepository _saveRepository;
    private User? _user = null;

    public HorsesController(IHorseRepository horseRepository, ISaveRepository saveRepository)
    {
        _horseRepository = horseRepository;
        _saveRepository = saveRepository;
    }

    [HttpGet()]
    [ProducesResponseType(typeof(ICollection<HorseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HorseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAsync(long? horseId, int below = 0, int amount = 20,
        string orderByDate = "desc", string orderByName = "false")
    {
        _user ??= (User)HttpContext.Items["user"]!;

        if (horseId == null)
        {
            var horses = await _horseRepository.GetAsync(_user.Id, below, amount, orderByDate, orderByName);

            if (horses.Count == 0)
            {
                return Ok(new List<HorseDto>());
            }
            else
            {
                return Ok(horses);
            }
        }
        else
        {
            var horse = await _horseRepository.GetByIdAsync(_user.Id, horseId.Value);

            if (horse is null)
            {
                return NotFound(new BadResponse(
                    Request.GetDisplayUrl(),
                    "NotFound",
                    HttpStatusCode.Unauthorized,
                    [new("Horse not found", "Horse not exists")]));
            }

            return Ok(horse);
        }
    }

    [HttpPost()]
    [Route("history")]
    public async Task<ActionResult<ICollection<ServiceResult<StorageHistory>>>> PushHistory(
        [FromBody] ICollection<StorageHistory> history)
    {
        _user ??= (User)HttpContext.Items["user"]!;

        var response = new List<ServiceResult<StorageHistory>>();

        // reformat history to Create/Update/Delete horse
        var historyHorsesGrouped = history.GroupBy(x => x.ActionType)
            .Where(x => x.Key == ActionType.CreateHorse || x.Key == ActionType.UpdateHorse || x.Key == ActionType.DeleteHorse)
            .ToList();

        foreach (var historyGroup in historyHorsesGrouped)
        {
            switch (historyGroup.Key)
            {
                case ActionType.CreateHorse:
                    foreach (var creatingHorseAction in historyGroup)
                    {
                        var creatingHorse = creatingHorseAction.Data as HorseDto ??
                            throw new ArgumentException("CreatingHorseAction is not HorseRetrievingDto");

                        var horseSaves = history.Where(x => x.Data is FullSaveDto saveDto && saveDto.HorseId == creatingHorse.HorseId);

                        var creatingHorseDto = new HorseCreatingDto()
                        {
                            Name = creatingHorse.Name,
                            Description = creatingHorse.Description,
                            Sex = creatingHorse.Sex,
                            BirthDate = creatingHorse.BirthDate,
                            City = creatingHorse.City,
                            Region = creatingHorse.Region,
                            Country = creatingHorse.Country,
                            OwnerName = creatingHorse.OwnerName,
                            OwnerPhoneNumber = creatingHorse.OwnerPhoneNumber,
                        };

                        foreach (var horseSave in horseSaves)
                        {
                            var horseSaveDto = horseSave.Data as FullSaveDto
                                ?? throw new ArgumentException("HorseSaveAction is not SaveCreatingDto");

                            var creatingSave = new HorseSaveCreatingDto()
                            {
                                Header = horseSaveDto.Header,
                                Description = horseSaveDto.Description,
                                Date = horseSaveDto.Date,
                                Bones = horseSaveDto.Bones
                            };

                            creatingHorseDto.Saves.Add(creatingSave);
                        }

                        foreach (var horseUser in creatingHorse.Users)
                        {
                            creatingHorseDto.Users.Add(new()
                            {
                                UserId = horseUser.UserId,
                                AccessRole = horseUser.AccessRole,
                                IsOwner = horseUser.IsOwner
                            });
                        }

                        var horseCreateResult = await _horseRepository.CreateAsync(_user, creatingHorseDto);

                        response.Add(new()
                        {
                            Success = horseCreateResult.Success,
                            ErrorMessage = horseCreateResult.ErrorMessage,
                            Result = creatingHorseAction
                        });
                    }
                    break;
                case ActionType.UpdateHorse:
                    foreach (var updatingHorseAction in historyGroup)
                    {
                        var updatingHorse = updatingHorseAction.Data as HorseDto ??
                            throw new ArgumentException("UpdatingHorseAction is not HorseRetrievingDto");

                        var updatingHorseDto = new HorseUpdatingDto()
                        {
                            HorseId = updatingHorse.HorseId,
                            Name = updatingHorse.Name,
                            Description = updatingHorse.Description,
                            Sex = updatingHorse.Sex,
                            BirthDate = updatingHorse.BirthDate,
                            City = updatingHorse.City,
                            Region = updatingHorse.Region,
                            Country = updatingHorse.Country,
                            OwnerName = updatingHorse.OwnerName,
                            OwnerPhoneNumber = updatingHorse.OwnerPhoneNumber,
                        };

                        foreach (var horseUser in updatingHorse.Users)
                        {
                            updatingHorseDto.Users.Add(new()
                            {
                                UserId = horseUser.UserId,
                                AccessRole = horseUser.AccessRole,
                                IsOwner = horseUser.IsOwner
                            });
                        }

                        var horseUpdateResult = await _horseRepository.UpdateAsync(_user, updatingHorseDto);

                        response.Add(new()
                        {
                            Success = horseUpdateResult.Success,
                            ErrorMessage = horseUpdateResult.ErrorMessage,
                            Result = updatingHorseAction
                        });

                        var horseSavesGroup = history.Where(x => x.Data is FullSaveDto saveDto && saveDto.HorseId == updatingHorse.HorseId)
                            .GroupBy(x => x.ActionType);

                        foreach (var saveGroup in horseSavesGroup)
                        {
                            switch (saveGroup.Key)
                            {
                                case ActionType.CreateSave:
                                    foreach (var saveAction in saveGroup)
                                    {
                                        var save = saveAction.Data as FullSaveDto ??
                                            throw new ArgumentException("SaveAction is not SaveCreatingDto");
                                        var creatingSave = new SaveCreatingDto()
                                        {
                                            HorseId = updatingHorse.HorseId,
                                            Header = save.Header,
                                            Date = save.Date,
                                            Description = save.Description,
                                        };

                                        foreach (var bone in save.Bones)
                                        {
                                            creatingSave.Bones.Add(new()
                                            {
                                                BoneId = bone.BoneId,
                                                Position = bone.Position,
                                                Rotation = bone.Rotation,
                                            });
                                        }

                                        var saveCreateResult = await _saveRepository.CreateToExistHorseAsync(creatingSave, _user.Id);

                                        response.Add(new()
                                        {
                                            Success = saveCreateResult.Success,
                                            ErrorMessage = saveCreateResult.ErrorMessage,
                                            Result = saveAction
                                        });
                                    }
                                    break;
                                case ActionType.UpdateSave:
                                    foreach (var saveAction in saveGroup)
                                    {
                                        var save = saveAction.Data as FullSaveDto ??
                                            throw new ArgumentException("SaveAction is not SaveCreatingDto");
                                        var updatingSave = new RequestUpdateSaveDto()
                                        {
                                            SaveId = save.SaveId,
                                            Header = save.Header,
                                            Date = save.Date,
                                            Description = save.Description,
                                        };

                                        var saveUpdateResult = await _saveRepository.UpdateAsync(updatingSave, _user.Id);

                                        response.Add(new()
                                        {
                                            Success = saveUpdateResult.Success,
                                            ErrorMessage = saveUpdateResult.ErrorMessage,
                                            Result = saveAction
                                        });
                                    }
                                    break;
                                case ActionType.DeleteSave:
                                    foreach (var saveAction in saveGroup)
                                    {
                                        var save = saveAction.Data as FullSaveDto ??
                                            throw new ArgumentException("SaveAction is not SaveCreatingDto");

                                        var saveDeleteResult = await _saveRepository.DeleteAsync(save.SaveId, _user.Id);

                                        response.Add(new()
                                        {
                                            Success = saveDeleteResult,
                                            Result = saveAction
                                        });
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                case ActionType.DeleteHorse:
                    foreach (var deletingHorseAction in historyGroup)
                    {
                        var deletingHorse = deletingHorseAction.Data as HorseDto ??
                            throw new ArgumentException("DeletingHorseAction is not HorseRetrievingDto");

                        var deletingHorseResult = await _horseRepository.DeleteAsync(deletingHorse.HorseId, _user.Id);

                        response.Add(new()
                        {
                            Success = deletingHorseResult,
                            Result = deletingHorseAction,
                        });
                    }
                    break;
            }
        }

        var result = await _horseRepository.SaveChangesAsync();

        if (result == null)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "SaveChangesError",
                HttpStatusCode.BadRequest,
                [new("Exception", "Save changes error")]));
        }

        return response;
    }

    [HttpPost]
    [ProducesResponseType(typeof(HorseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HorseDto>> CreateAsync([FromBody] HorseCreatingDto requestHorse)
    {
        _user ??= (User)HttpContext.Items["user"]!;
        var creatingResult = await _horseRepository.CreateAsync(_user, requestHorse);

        if (!creatingResult.Success)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "AddHorseError",
                HttpStatusCode.BadRequest,
                [new("Exception", creatingResult.ErrorMessage!)]));
        }

        await _horseRepository.SaveChangesAsync();
        return Created(HttpContext.Request.GetDisplayUrl(), creatingResult.Result!);
    }

    [HttpPut]
    [ProducesResponseType(typeof(HorseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
    [TypeFilter(typeof(AccessRoleFilter), Arguments = [UserAccessRole.Writer])]
    public async Task<ActionResult<HorseDto>> UpdateAsync([FromBody] HorseUpdatingDto requestHorse)
    {
        _user ??= (User)HttpContext.Items["user"]!;
        var updatingResult = await _horseRepository.UpdateAsync(_user, requestHorse);

        if (!updatingResult.Success)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "UpdateHorseError",
                HttpStatusCode.BadRequest,
                [new("Exception", updatingResult.ErrorMessage!)]));
        }

        await _horseRepository.SaveChangesAsync();
        return Ok(updatingResult.Result!);
    }

    [HttpDelete]
    [TypeFilter(typeof(AccessRoleFilter), Arguments = [UserAccessRole.Writer])]
    public async Task<IActionResult> DeleteAsync(long horseId)
    {
        var horseUser = (HorseUser)HttpContext.Items["horseUser"]!;
        var deletingResult = await _horseRepository.DeleteAsync(horseId, horseUser.UserId);

        if (!deletingResult)
        {
            return BadRequest();
        }

        var result = await _horseRepository.SaveChangesAsync();

        if (result == 0)
        {
            return BadRequest();
        }

        return Ok();
    }
}

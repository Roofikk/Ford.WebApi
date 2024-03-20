using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Dtos.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Data.Entity;

namespace Ford.WebApi.Filters
{
    public class AccessRoleFilter : Attribute, IAsyncActionFilter
    {
        private readonly FordContext _context;
        private readonly UserAccessRole _minimalAccessRole;

        public AccessRoleFilter(FordContext context, UserAccessRole userAccessRole)
        {
            _context = context;
            _minimalAccessRole = userAccessRole;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = (User)context.HttpContext.Items["user"]!;

            UserHorse? horseUser = null;
            
            if (context.ActionArguments["requestSave"] is IRequestSave requestSave)
            {
                switch (requestSave)
                {
                    case RequestCreateSaveDto requestCreateSaveDto:
                        horseUser = await _context.HorseUsers.SingleOrDefaultAsync(u => u.UserId == user.Id && u.HorseId == requestCreateSaveDto.HorseId);
                        break;

                    case RequestUpdateSaveDto requestUpdateSaveDto:
                        var save = await _context.Saves.SingleOrDefaultAsync(s => s.SaveId == requestUpdateSaveDto.SaveId);
                        horseUser = await _context.HorseUsers.SingleOrDefaultAsync(u => u.UserId == user.Id && u.HorseId == save.HorseId);
                        break;
                }
            }

            if (context.ActionArguments["saveId"] is long saveId)
            {
                var save = await _context.Saves.SingleOrDefaultAsync(s => s.SaveId == saveId);
                horseUser = await _context.HorseUsers.SingleOrDefaultAsync(u => u.UserId == user.Id && u.HorseId == save.HorseId);
            }

            if (context.ActionArguments["requestHorse"] is RequestUpdateHorseDto horse)
            {
                horseUser = await _context.HorseUsers.SingleOrDefaultAsync(u => u.UserId == user.Id && u.HorseId == horse.HorseId);
            }

            if (horseUser == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            var accessRole = Enum.Parse<UserAccessRole>(horseUser.RuleAccess);

            if (accessRole < _minimalAccessRole)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            context.HttpContext.Items.Add("horseUser", horseUser);
            await next();
        }
    }
}

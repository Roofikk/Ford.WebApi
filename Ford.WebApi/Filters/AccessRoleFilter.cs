using Ford.WebApi.Data;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Dtos.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;


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

            HorseUser? horseUser = null;
            
            if (context.ActionArguments.TryGetValue("requestSave", out var value))
            {
                var requestSave = value as IRequestSave; 
                
                switch (requestSave)
                {
                    case SaveCreatingDto requestCreateSaveDto:
                        horseUser = await _context.HorseUsers.SingleOrDefaultAsync(u => u.UserId == user.Id && u.HorseId == requestCreateSaveDto.HorseId);
                        break;

                    case RequestUpdateSaveDto requestUpdateSaveDto:
                        var save = await _context.Saves.SingleOrDefaultAsync(s => s.SaveId == requestUpdateSaveDto.SaveId);
                        horseUser = await _context.HorseUsers.SingleOrDefaultAsync(u => u.UserId == user.Id && u.HorseId == save!.HorseId);
                        break;
                }
            }

            if (context.ActionArguments.TryGetValue("saveId", out value))
            {
                var saveId = Convert.ToInt64(value);
                var save = await _context.Saves.SingleOrDefaultAsync(s => s.SaveId == saveId);
                horseUser = await _context.HorseUsers.SingleOrDefaultAsync(u => u.UserId == user.Id && u.HorseId == save!.HorseId);
            }

            if (context.ActionArguments.TryGetValue("requestHorse", out value))
            {
                var requestHorse = value as HorseUpdatingDto;
                horseUser = await _context.HorseUsers.SingleOrDefaultAsync(u => u.UserId == user.Id && u.HorseId == requestHorse!.HorseId);
            }

            if (context.ActionArguments.TryGetValue("horseId", out value))
            {
                var horseId = Convert.ToInt64(value);
                horseUser = await _context.HorseUsers.SingleOrDefaultAsync(u => u.UserId == user.Id && u.HorseId == horseId);
            }

            if (horseUser == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            var accessRole = Enum.Parse<UserAccessRole>(horseUser.AccessRole);

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

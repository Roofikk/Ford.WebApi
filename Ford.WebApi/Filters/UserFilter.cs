using Ford.WebApi.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ford.WebApi.Filters
{
    public class UserFilter : Attribute, IAsyncActionFilter
    {
        private UserManager<User> _userManager;

        public UserFilter(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = await _userManager.GetUserAsync(context.HttpContext.User);

            if (user == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }

            context.HttpContext.Items.Add("user", user); 

            await next();
        }
    }
}

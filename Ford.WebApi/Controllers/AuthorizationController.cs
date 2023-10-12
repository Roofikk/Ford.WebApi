using Ford.WebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthorizationController : ControllerBase
{
    [HttpPost]
    public IActionResult Registration(User user)
    {
        return Ok();
    }
}

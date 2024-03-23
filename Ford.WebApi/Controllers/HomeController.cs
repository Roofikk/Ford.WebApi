using Microsoft.AspNetCore.Mvc;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HomeController : ControllerBase
{
    [HttpGet()]
    public IActionResult Index()
    {
        return Ok("Hello world");
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ford.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Ford.WebApi.Dtos.User;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Data;
using System.Data.Entity;
using Ford.WebApi.Services.Identity;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IdentityController : ControllerBase
{
    private readonly FordContext db;
    private readonly UserManager<User> userManager;
    private readonly ITokenService tokenService;

    private static readonly TimeSpan tokenLifeTime = TimeSpan.FromDays(14);

    public IdentityController(FordContext db, ITokenService tokenService, UserManager<User> userManager)
    {
        this.db = db;
        this.tokenService = tokenService;
        this.userManager = userManager;
    }

    [HttpPost()]
    [Route("register")]
    public async Task<ActionResult<UserGettingDto>> Register([FromBody] UserRegister request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(request);
        }

        User user = new User
        {
            UserName = request.Login,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            BirthDate = request.BirthDate,
            CreationDate = DateTime.Now,
            LastUpdatedDate = DateTime.Now
        };
        var result = await userManager.CreateAsync(user, request.Password);

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }
        
        if (!result.Succeeded)
        {
            return BadRequest(request);
        }

        var findUser = db.Users.FirstOrDefault(u => u.UserName == request.Login);

        if (findUser == null)
        {
            throw new Exception($"User {request.Email} not found");
        }

        await userManager.AddToRoleAsync(findUser, Roles.Member);

        return new UserGettingDto
        {
            UserId = user.Id.ToString(),
            Login = request.Login,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request?.LastName,
            BirthDate = request?.BirthDate,
            CreationDate = user.CreationDate
        };
    }

    [HttpPost()]
    [Route("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] UserLogin request)
    {
        User managedUser = await userManager.FindByNameAsync(request.Login);

        if (managedUser is null)
        {
            return NotFound("User not found");
        }

        bool isPasswordValid = await userManager.CheckPasswordAsync(managedUser, request.Password);

        if (!isPasswordValid)
        {
            return Unauthorized();
        }

        User? user = db.Users.FirstOrDefault(u => u.UserName == request.Login);

        if (user is null)
        {
            return Unauthorized();
        }
        
        List<long> roleIds = db.UserRoles.Where(r => r.UserId == user.Id)
            .Select(x => x.RoleId).ToList();

        var roles = db.Roles.Where(x => roleIds.Contains(x.Id)).ToList();
        string? jwtToken = tokenService.GenerateToken(user, roles, tokenLifeTime);
        // should encrypt this token and after return to user

        return new AuthResponse
        {
            Login = request.Login,
            Email = user.Email,
            Token = jwtToken,
        };
    }

    [Authorize]
    [HttpGet("token")]
    public IActionResult CheckToken()
    {
        return Ok();
    }
}

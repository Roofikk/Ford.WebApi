using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ford.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Data;
using System.Data.Entity;
using Ford.WebApi.Services.Identity;
using Ford.WebApi.Dtos.User;
using System.Security.Claims;
using AutoMapper;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IdentityController : ControllerBase
{
    private readonly FordContext db;
    private readonly UserManager<User> userManager;
    private readonly ITokenService tokenService;
    private readonly IMapper mapper;

    private static readonly TimeSpan tokenLifeTime = TimeSpan.FromDays(14);

    public IdentityController(FordContext db, ITokenService tokenService, UserManager<User> userManager, IMapper mapper)
    {
        this.db = db;
        this.tokenService = tokenService;
        this.userManager = userManager;
        this.mapper = mapper;
    }

    [HttpPost()]
    [Route("register")]
    public async Task<ActionResult<UserGettingDto>> Register([FromBody]UserRegister request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(request);
        }

        User user = new User()
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

        var findUser = await db.Users.FirstOrDefaultAsync(u => u.UserName == request.Login);

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
    [HttpGet("identity")]
    public async Task<ActionResult<UserGettingDto>> GetUserInfo()
    {
        string? jwtToken = Request.Headers["Authorization"];
        ClaimsPrincipal? principal = tokenService.GetPrincipalFromToken(jwtToken.Replace("Bearer ", string.Empty));

        if (principal == null)
        {
            return Unauthorized("Invalid access token");
        }

        string? userName = principal.Identity!.Name;

        User? user = await userManager.FindByNameAsync(userName);

        if (user is null)
        {
            return BadRequest("Invalid access token");
        }

        UserGettingDto userDto = mapper.Map<UserGettingDto>(user);
        return userDto;
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Data;
using Ford.WebApi.Services.Identity;
using Ford.WebApi.Dtos.User;
using System.Security.Claims;
using AutoMapper;
using Ford.WebApi.Models.Identity;

namespace Ford.WebApi.Controllers;

[Authorize]
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

    [AllowAnonymous]
    [HttpPost]
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
        IdentityResult result = await userManager.CreateAsync(user, request.Password);

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
        var userDto = mapper.Map<UserGettingDto>(user);

        return userDto;
    }

    [AllowAnonymous]
    [HttpPost]
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

    [HttpGet]
    [Route("/api/account")]
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

    [HttpPost]
    [Route("/api/account")]
    public async Task<ActionResult<UserGettingDto>> Update([FromBody] UpdateUserRequest request)
    {
        if (!Request.Headers.TryGetValue("Authorization", out var token))
        {
            return Unauthorized("Invalid access token");
        }
        else
        {
            User? user = await tokenService.GetUserByToken(token);

            if (user is null)
            {
                return Unauthorized("Invalid access token");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(request);
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                //Learn about update phone number
                await userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
            }

            user.FirstName = request.FirstName;
            user.LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName;
            user.City = string.IsNullOrEmpty(request.City) ? null : request.City;
            user.Region = string.IsNullOrEmpty(request.Region) ? null : request.Region;
            user.Country = string.IsNullOrEmpty(request.Country) ? null : request.Country;
            user.BirthDate = request.BirthDate is null ? null : request.BirthDate;
            user.LastUpdatedDate = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Ok(request);
        }
    }

    [HttpPost]
    [Route("/api/account/password")]
    public async Task<ActionResult<AuthResponse>> ChangePassword([FromBody] RequestChangePassword request)
    {
        if (!Request.Headers.TryGetValue("Authorization", out var token))
        {
            return Unauthorized("Invalid access token");
        }
        else
        {
            if (request.CurrentPassword == request.NewPassword)
            {
                return BadRequest("Current password and new password is equal");
            }

            User? user = await tokenService.GetUserByToken(token);

            if (user is null)
            {
                return Unauthorized("Invalid access token");
            }

            IdentityResult result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            if (!result.Succeeded)
            {
                return BadRequest();
            }

            return await Login(new UserLogin
            {
                Login = user.UserName,
                Password = request.NewPassword
            });
        }
    }
}

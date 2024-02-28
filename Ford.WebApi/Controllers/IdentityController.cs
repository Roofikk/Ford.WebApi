using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Data;
using Ford.WebApi.Services.Identity;
using Ford.WebApi.Dtos.User;
using AutoMapper;
using Ford.WebApi.Models.Identity;
using Ford.WebApi.Dtos.Response;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.ObjectModel;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IdentityController : ControllerBase
{
    private readonly FordContext db;
    private readonly UserManager<User> userManager;
    private readonly RoleManager<IdentityRole<long>> roleManager;
    private readonly ITokenService tokenService;
    private readonly IMapper mapper;

    private static readonly TimeSpan tokenLifeTime = TimeSpan.FromHours(8);

    public IdentityController(FordContext db, ITokenService tokenService, UserManager<User> userManager,
        RoleManager<IdentityRole<long>> roleManager, IMapper mapper)
    {
        this.db = db;
        this.tokenService = tokenService;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.mapper = mapper;
    }

    [HttpPost]
    [Route("sign-up")]
    [ProducesResponseType(typeof(UserGettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserGettingDto>> SignUp([FromBody] UserRegister request)
    {
        // надо разобраться, как вообще работает ModelState и как он может быть невалидным
        if (!ModelState.IsValid)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Model state",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("Invalid data", "Model state is invalid. Check correctly input.") }));
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

        if (!result.Succeeded)
        {
            Collection<Error> responseErrors = new();

            foreach (var error in result.Errors)
            {
                responseErrors.Add(new(error.Code, error.Description));
            }

            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Sign up failed",
                HttpStatusCode.BadRequest,
                responseErrors));
        }

        var findUser = db.Users.FirstOrDefault(u => u.UserName == request.Login) 
            ?? throw new Exception($"User {request.Login} not found");

        IdentityRole<long>? memberRoleIdentity = await roleManager.FindByNameAsync(Roles.Member);

        if (memberRoleIdentity == null)
        {
            var createdRoleResult = await roleManager.CreateAsync(new IdentityRole<long>(Roles.Member));
        }

        await userManager.AddToRoleAsync(findUser, Roles.Member);
        var userDto = mapper.Map<UserGettingDto>(user);

        return userDto;
    }

    [HttpPost]
    [Route("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] UserLogin request)
    {
        User? user = await userManager.FindByNameAsync(request.Login);

        if (user is null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Authorization",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("Invalid Authorization", "Login or password incorrect") }));
        }

        bool isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Authorization",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("Invalid Authorization", "Login or password incorrect") }));
        }

        string jwtToken = await tokenService.GenerateToken(user, tokenLifeTime);

        return new AuthResponse
        {
            Login = request.Login,
            Token = jwtToken,
        };
    }

    [HttpGet, Authorize]
    [Route("check")]
    public IActionResult CheckAuth()
    {
        return Ok();
    }

    [HttpGet, Authorize]
    [Route("account")]
    [ProducesResponseType(typeof(UserGettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserGettingDto>> GetUserInfo()
    {
        string jwtToken = Request.Headers.Authorization!;

        User? user = await tokenService.GetUserByPrincipal(User);

        if (user is null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Authorization",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("User's token", "Token is invalid") }));
        }

        UserGettingDto userDto = mapper.Map<UserGettingDto>(user);
        return userDto;
    }

    [HttpPost, Authorize]
    [Route("account")]
    [ProducesResponseType(typeof(UserGettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserGettingDto>> Update([FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Invalid data",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("Invalid data", "Body content is incorrect") }));
        }

        User? user = await tokenService.GetUserByPrincipal(User);

        if (user is null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Authorization",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("User's token", "Token is invalid") }));
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.City = request.City;
        user.Region = request.Region;
        user.Country = request.Country;
        user.BirthDate = request.BirthDate is null ? null : request.BirthDate;
        user.LastUpdatedDate = DateTime.UtcNow;

        await db.SaveChangesAsync();

        UserGettingDto userDto = mapper.Map<UserGettingDto>(user);
        return Ok(userDto);
    }

    [HttpPost, Authorize]
    [Route("/account/password")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> ChangePassword([FromBody] RequestChangePassword request)
    {
        User? user = await tokenService.GetUserByPrincipal(User);

        if (user is null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Authorization",
                HttpStatusCode.Unauthorized,
                new Collection<Error> { new("User's token", "Token is invalid") }));
        }

        IdentityResult result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            Collection<Error> responseErrors = new();

            foreach (var error in result.Errors)
            {
                responseErrors.Add(new(error.Code, error.Description));
            }

            return BadRequest(new BadResponse(
                Request.GetDisplayUrl(),
                "Sign up failed",
                HttpStatusCode.BadRequest,
                responseErrors));
        }

        return await Login(new UserLogin
        {
            Login = user.UserName!,
            Password = request.NewPassword
        });
    }
}

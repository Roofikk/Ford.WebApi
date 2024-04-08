using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Data;
using Ford.WebApi.Services.Identity;
using Ford.WebApi.Dtos.User;
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
    private readonly FordContext _context;
    private readonly UserManager<User> userManager;
    private readonly RoleManager<IdentityRole<long>> roleManager;
    private readonly ITokenService tokenService;

    public IdentityController(FordContext context, ITokenService tokenService, 
        UserManager<User> userManager, RoleManager<IdentityRole<long>> roleManager)
    {
        this._context = context;
        this.tokenService = tokenService;
        this.userManager = userManager;
        this.roleManager = roleManager;
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
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            BirthDate = request.BirthDate,
            CreationDate = DateTime.UtcNow,
            LastUpdate = DateTime.UtcNow
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

        var findUser = await userManager.FindByNameAsync(request.UserName) 
            ?? throw new Exception($"User {request.UserName} not found");

        IdentityRole<long>? memberRoleIdentity = await roleManager.FindByNameAsync(Roles.Member);

        if (memberRoleIdentity == null)
        {
            var createdRoleResult = await roleManager.CreateAsync(new IdentityRole<long>(Roles.Member));
        }

        await userManager.AddToRoleAsync(findUser, Roles.Member);

        UserGettingDto userDto = new()
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BirthDate = user.BirthDate,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            Region = user.Region,
            Country = user.Country,
            CreationDate = user.CreationDate,
            LastUpdatedDate = user.LastUpdate,
        };

        return userDto;
    }

    [HttpPost]
    [Route("login")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenDto>> Login([FromBody] UserLogin request)
    {
        User? user = await userManager.FindByNameAsync(request.UserName);

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

        var token = await tokenService.GenerateTokenAsync(user);

        user.RefreshToken = token.RefreshToken;
        user.RefreshTokenExpiresDate = token.ExpiredDate;
        await userManager.UpdateAsync(user);

        return new TokenDto
        {
            Token = token.JwtToken,
            RefreshToken= token.RefreshToken
        };
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenDto>> RefreshToken(TokenDto requset)
    {
        var principal = tokenService.GetPrincipalFromExpiredToken(requset.Token);
        var user = await userManager.GetUserAsync(principal);

        if (user == null || user.RefreshToken != requset.RefreshToken || user.RefreshTokenExpiresDate <= DateTime.UtcNow)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Unauthorized",
                HttpStatusCode.Unauthorized,
                [new("Refresh token denied", "Refresh token denied")]));
        }

        var token = await tokenService.GenerateTokenAsync(user);

        user.RefreshToken = token.RefreshToken;
        user.RefreshTokenExpiresDate = token.ExpiredDate;
        await userManager.UpdateAsync(user);

        return new TokenDto
        {
            Token = token.JwtToken,
            RefreshToken = token.RefreshToken,
        };
    }

    [HttpGet]
    [Authorize]
    [Route("check-token")]
    public IActionResult CheckToken()
    {
        return Ok();
    }

    [HttpGet, Authorize]
    [Route("account")]
    [ProducesResponseType(typeof(UserGettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserGettingDto>> GetUserInfo()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Authorization",
                HttpStatusCode.Unauthorized,
                [new("User's token", "Token is invalid")]));
        }

        UserGettingDto userDto = new()
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BirthDate = user.BirthDate,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            Region = user.Region,
            Country = user.Country,
            CreationDate = user.CreationDate,
            LastUpdatedDate = user.LastUpdate,
        };
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
                [new("Invalid data", "Body content is incorrect")]));
        }

        User? user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Authorization",
                HttpStatusCode.Unauthorized,
                [new("User's token", "Token is invalid")]));
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.City = request.City;
        user.Region = request.Region;
        user.PhoneNumber = request.PhoneNumber;
        user.Country = request.Country;
        user.BirthDate = request.BirthDate is null ? null : request.BirthDate;
        user.LastUpdate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        UserGettingDto userDto = new()
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BirthDate = user.BirthDate,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            Region = user.Region,
            Country = user.Country,
            CreationDate = user.CreationDate,
            LastUpdatedDate = user.LastUpdate,
        };
        return Ok(userDto);
    }

    [HttpPost, Authorize]
    [Route("account/password")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BadResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TokenDto>> ChangePassword([FromBody] RequestChangePassword request)
    {
        User? user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized(new BadResponse(
                Request.GetDisplayUrl(),
                "Authorization",
                HttpStatusCode.Unauthorized,
                [new("User's token", "Token is invalid")]));
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
            UserName = user.UserName!,
            Password = request.NewPassword
        });
    }
}

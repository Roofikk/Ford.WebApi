using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ford.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Ford.WebApi.PasswordHasher;
using Microsoft.AspNetCore.Identity;
using Ford.WebApi.Dtos.User;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Data;
using System.Data.Entity;
using System.Globalization;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IdentityController : ControllerBase
{
    private readonly FordContext db;
    private readonly UserManager<User> userManager;
    private readonly IConfiguration configuration;
    private readonly IPasswordHasher passwordHasher;

    private static readonly TimeSpan tokenLifeTime = TimeSpan.FromDays(14);

    public IdentityController(FordContext db, IConfiguration configuration, 
        IPasswordHasher passwordHasher, UserManager<User> userManager)
    {
        this.db = db;
        this.configuration = configuration;
        this.passwordHasher = passwordHasher;
        this.userManager = userManager;
    }

    [HttpPost()]
    [Route("register")]
    public async Task<ActionResult<UserGettingDto>> SignUp([FromBody]UserSignUp request)
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
    public async Task<ActionResult<string>> Login([FromBody] UserSignIn request)
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

        var tokenHandler = new JwtSecurityTokenHandler();
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);

        List<Claim> claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new (ClaimTypes.NameIdentifier, managedUser.Id.ToString()),
            new (ClaimTypes.Email, managedUser.Email!),
            new (ClaimTypes.Name, managedUser.UserName),
            new (ClaimTypes.Role, string.Join(",", roles.Select(r => r.Name)))
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(tokenLifeTime),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        // should encrypt this token and after return to user
        return Ok(jwtToken);
    }

    [Authorize]
    [HttpGet("token")]
    public IActionResult CheckToken()
    {
        return Ok();
    }
}

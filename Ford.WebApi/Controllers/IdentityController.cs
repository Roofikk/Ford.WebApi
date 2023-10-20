using Ford.Common.EntityModels.Models;
using Ford.DataContext.Sqlite;
using Ford.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IdentityController : ControllerBase
{
    private readonly FordContext db;
    private readonly IConfiguration configuration;

    private static readonly TimeSpan tokenLifeTime = TimeSpan.FromHours(2);

    public IdentityController(FordContext db, IConfiguration configuration)
    {
        this.db = db;
        this.configuration = configuration;
    }

    [HttpPost("auth")]
    public async Task<IActionResult> Registration([FromBody]RegistrationUser user)
    {
        if (user is null)
        {
            return BadRequest("User is null");
        }

        if (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest("Login or password can not be empty");
        }

        User? existingUser = await db.Users.FirstOrDefaultAsync(u => u.Login == user.Login);

        if (existingUser is not null)
        {
            return Conflict($"User with {user.Login} login is existing");
        }

        db.Users.Add(new User
        {
            Name = user.Name,
            Email = user.Email,
            PasswordHash = user.Password, // EDIT!!!
            Role = "User",
            UserId = Guid.NewGuid().ToString(),
            CreationDate = DateTime.Now
        });
        db.SaveChanges();

        return Ok();
    }

    // check success existing user hash and get secret encrypting token
    // user should decrypt token on our local machine
    [HttpPost("token")]
    public IActionResult GetToken([FromBody]TokenGenerationRequest request)
    {
        if (db.Users.SingleOrDefault(u => u.UserId == request.UserId) is null)
        {
            return NotFound("User not found");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);

        List<Claim> claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Sub, request.UserId),
            new (ClaimTypes.Email, request.Email ?? ""),
            new (ClaimTypes.Name, request.Login),
            new (ClaimTypes.Role, request.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),

            Expires = DateTime.UtcNow.Add(tokenLifeTime),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        // should encrypt this token and after return to user
        return Ok(jwtToken);
    }

    public static List<Claim> DecodeToken()
    {


        return new List<Claim>();
    }

    [Authorize]
    [HttpGet("token")]
    public IActionResult CheckToken()
    {
        return Ok();
    }
}

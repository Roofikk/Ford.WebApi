using Ford.DataContext.Sqlite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ford.EntityModels.Models;
using Ford.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Ford.WebApi.Repositories.PasswordHasher;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IdentityController : ControllerBase
{
    private readonly FordContext db;
    private readonly IConfiguration configuration;
    private readonly IPasswordHasher passwordHasher;

    private static readonly TimeSpan tokenLifeTime = TimeSpan.FromHours(2);

    public IdentityController(FordContext db, IConfiguration configuration, IPasswordHasher passwordHasher)
    {
        this.db = db;
        this.configuration = configuration;
        this.passwordHasher = passwordHasher;
    }

    [HttpPost("auth")]
    public async Task<IActionResult> SignUp([FromBody]UserSignUp user)
    {
        if (user is null)
        {
            return BadRequest("User is null");
        }

        User? existingUser = await db.Users.FirstOrDefaultAsync(u => u.Login == user.Login);

        if (existingUser is not null)
        {
            return Conflict();
        }

        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] saltBytes = new byte[16];
        rng.GetBytes(saltBytes);
        string salt = Convert.ToBase64String(saltBytes);

        db.Users.Add(new User
        {
            UserId = Guid.NewGuid().ToString(),
            Login = user.Login,
            Salt = salt,
            HashedPassword = passwordHasher.Hash(user.Password),
            Name = user.Name,
            Email = user.Email
        });

        if ((await db.SaveChangesAsync()) == 1)
        {
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }

    // check success existing user hash and get secret encrypting token
    // user should decrypt token on our local machine
    [HttpPost("token")]
    public async Task<ActionResult<string>> SignIn([FromBody]UserSignIn request)
    {
        User? user = await db.Users.SingleOrDefaultAsync(u => u.Login == request.Login);

        if (user is null)
        {
            return NotFound("User not found");
        }

        if (!passwordHasher.Verify(user.HashedPassword, request.Password))
        {
            return Unauthorized();
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);

        List<Claim> claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Sub, user.UserId),
            new (ClaimTypes.Email, user.Email ?? ""),
            new (ClaimTypes.Name, user.Login),
            new (ClaimTypes.Role, user.Role)
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

    [Authorize]
    [HttpGet("token")]
    public IActionResult CheckToken()
    {
        return Ok();
    }
}

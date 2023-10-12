using Ford.DataContext.Sqlite;
using Ford.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ford.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthorizationController : ControllerBase
{
    private readonly FordContext db;
    private readonly IConfiguration configuration;

    public AuthorizationController(FordContext db, IConfiguration configuration)
    {
        this.db = db;
        this.configuration = configuration;
    }

    [HttpPost]
    public IActionResult Registration([FromBody] User user)
    {
        if (user is null)
        {
            return BadRequest("User is null");
        }

        if (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest("Login or password can not be empty");
        }

         User? existingUser = db.Users.FirstOrDefault(u => u.Login == user.Login);

        if (existingUser is not null)
        {
            return Conflict($"User with {user.Login} login is existing");
        }

        db.Users.Add(user);
        db.SaveChanges();

        return Ok();
    }

    [HttpGet]
    public IActionResult GetToken(string userHash)
    {
        // check success existing user hash and get secret encrypting token
        // user should decrypt token on our local machine
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.CHash, userHash),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),

            Expires = DateTime.UtcNow.AddYears(1),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);
        var stringToken = tokenHandler.WriteToken(token);

        // should encrypt this token and after return to user
        return Ok(stringToken);
    }
}

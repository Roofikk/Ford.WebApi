using Ford.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ford.WebApi.Dtos.User;

public class UserForUpdateDto
{
    public string UserId { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Email { get; set; }
    public string Name { get; set; } = null!;
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public DateTime? BirthDate { get; set; }
}
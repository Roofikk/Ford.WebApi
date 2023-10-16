using Ford.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ford.WebApi.Dtos.User;

public class UserCreationDto
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public DateTime? DirthDate { get; set; }
    public DateTime RegistrationDate { get; set; }
    public ICollection<Horse>? Horses { get; set; }
}
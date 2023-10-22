namespace Ford.WebApi.Models
{
    public class UserRegister
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set;} = null!;
        public string? LastName { get; set; }
        public string Email { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
    }
}

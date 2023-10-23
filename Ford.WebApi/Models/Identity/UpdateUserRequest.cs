namespace Ford.WebApi.Models.Identity
{
    public class UpdateUserRequest
    {
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}

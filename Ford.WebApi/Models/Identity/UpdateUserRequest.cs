namespace Ford.WebApi.Models.Identity
{
    public class UpdateUserRequest
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Region { get; set; } = null!;
        public string Country { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
    }
}

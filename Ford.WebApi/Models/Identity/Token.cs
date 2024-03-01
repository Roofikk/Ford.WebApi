namespace Ford.WebApi.Models.Identity
{
    public class Token
    {
        public string JwtToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiredDate { get; set; }
    }
}

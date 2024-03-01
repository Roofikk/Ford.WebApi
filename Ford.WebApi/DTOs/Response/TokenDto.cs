namespace Ford.WebApi.Dtos.Response
{
    public class TokenDto
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}

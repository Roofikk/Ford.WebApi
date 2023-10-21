namespace Ford.WebApi.PasswordHasher
{
    public interface IPasswordHasher
    {
        public string Hash(string password);
        public bool Verify(string hashedPassword, string inputPassword);
    }
}

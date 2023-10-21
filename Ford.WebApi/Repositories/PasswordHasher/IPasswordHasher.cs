namespace Ford.WebApi.Repositories.PasswordHasher
{
    public interface IPasswordHasher
    {
        public string Hash(string password);
        public bool Verify(string hashedPassword, string inputPassword);
    }
}

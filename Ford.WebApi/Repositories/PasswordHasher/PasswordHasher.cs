using System.Security.Cryptography;

namespace Ford.WebApi.Repositories.PasswordHasher;

public class PasswordHasher : IPasswordHasher
{
    private const int saltSize = 128 / 8;
    private const int keySize = 256 / 8;
    private const int iterations = 100000;
    private static readonly HashAlgorithmName _hashLagorithmName = HashAlgorithmName.SHA256;
    private const char Delimeter = '.';

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(saltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            _hashLagorithmName,
            keySize);

        return string.Join(Delimeter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool Verify(string hashedPassword, string inputPassword)
    {
        string[] elements = hashedPassword.Split('.');
        byte[] salt = Convert.FromBase64String(elements[0]);
        byte[] hash = Convert.FromBase64String(elements[1]);

        byte[] hashInput = Rfc2898DeriveBytes.Pbkdf2(
            inputPassword,
            salt,
            iterations,
            _hashLagorithmName,
            keySize);

        return CryptographicOperations.FixedTimeEquals(hash, hashInput);
    }
}

using System.Security.Cryptography;
using EnterpriseCRM.Application.Common.Interfaces;

namespace EnterpriseCRM.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32; // 256-bit salt
    private const int HashSize = 64; // 512-bit key
    private const int Iterations = 350_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public (string Hash, string Salt) HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            Algorithm,
            HashSize);

        var hash = Convert.ToBase64String(hashBytes);
        var salt = Convert.ToBase64String(saltBytes);

        return (hash, salt);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(hash) ||
            string.IsNullOrWhiteSpace(salt))
        {
            return false;
        }

        try
        {
            var saltBytes = Convert.FromBase64String(salt);
            var expectedHashBytes = Convert.FromBase64String(hash);

            var computedHashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                Iterations,
                Algorithm,
                expectedHashBytes.Length);

            return CryptographicOperations.FixedTimeEquals(computedHashBytes, expectedHashBytes);
        }
        catch
        {
            return false;
        }
    }
}

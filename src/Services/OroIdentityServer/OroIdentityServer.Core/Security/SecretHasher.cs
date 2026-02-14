using System.Security.Cryptography;

namespace OroIdentityServer.Core.Security;

public static class SecretHasher
{
    // Format: {iterations}.{salt-base64}.{hash-base64}
    public static string Hash(string secret, int iterations = 100_000)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        using var pbk = new Rfc2898DeriveBytes(secret, salt, iterations, HashAlgorithmName.SHA256);
        var hash = pbk.GetBytes(32);
        return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string secret, string stored)
    {
        try
        {
            var parts = stored.Split('.');
            if (parts.Length != 3) return false;
            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var expected = Convert.FromBase64String(parts[2]);
            using var pbk = new Rfc2898DeriveBytes(secret, salt, iterations, HashAlgorithmName.SHA256);
            var actual = pbk.GetBytes(expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch
        {
            return false;
        }
    }
}

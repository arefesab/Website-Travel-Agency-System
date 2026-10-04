using System;
using System.Security.Cryptography;

namespace agancywebProject.Services
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;   // 128 bit
        private const int KeySize = 32;    // 256 bit
        private const int Iterations = 100_000;

        public static string Hash(string password)
        {
            using var algorithm = new Rfc2898DeriveBytes(
                password, SaltSize, Iterations, HashAlgorithmName.SHA256);

            var salt = algorithm.Salt;
            var key = algorithm.GetBytes(KeySize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public static bool IsHashed(string? value)
        {
            return TryParse(value, out _, out _, out _);
        }

        public static bool Verify(string password, string? hashedValue)
        {
            if (!TryParse(hashedValue, out var iterations, out var salt, out var key))
            {
                return false;
            }

            using var algorithm = new Rfc2898DeriveBytes(
                password, salt, iterations, HashAlgorithmName.SHA256);
            var keyToCheck = algorithm.GetBytes(key.Length);

            return CryptographicOperations.FixedTimeEquals(keyToCheck, key);
        }

        private static bool TryParse(string? hashedValue, out int iterations, out byte[] salt, out byte[] key)
        {
            iterations = 0;
            salt = Array.Empty<byte>();
            key = Array.Empty<byte>();

            if (string.IsNullOrEmpty(hashedValue))
            {
                return false;
            }

            var parts = hashedValue.Split('.', 3);
            if (parts.Length != 3 || !int.TryParse(parts[0], out iterations) || iterations <= 0)
            {
                return false;
            }

            try
            {
                salt = Convert.FromBase64String(parts[1]);
                key = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            return salt.Length >= 8 && key.Length > 0;
        }
    }
}

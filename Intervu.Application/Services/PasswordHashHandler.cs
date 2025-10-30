using System.Security.Cryptography;

namespace Intervu.Application.Services
{
    public class PasswordHashHandler
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 10000;

        //Return a string of type: {iterations}.{salt}.{hash}
        public static string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            //Using Password-Based Key Derivation Function 2 (PBKDF2) with HMAC-SHA256
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(KeySize);
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        // Verify the password against the stored hash
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split('.', 3);
            if (parts.Length != 3)
            {
                return false;
            }
            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var storedHash = Convert.FromBase64String(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var hashToCompare = pbkdf2.GetBytes(KeySize);

            return CryptographicOperations.FixedTimeEquals(storedHash, hashToCompare);
        }
    }
}

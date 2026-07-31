using System.Security.Cryptography;

namespace Sifp_Vue.Server.Helpers
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string encodedHash);
    }

    /// <summary>
    /// PBKDF2-HMAC-SHA256 memakai API bawaan .NET, tanpa dependensi pihak ketiga.
    /// Format simpan: <c>{iterations}.{saltBase64}.{hashBase64}</c> — iterasi ikut
    /// disimpan supaya hash lama tetap bisa diverifikasi saat parameter dinaikkan.
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int DefaultIterations = 210_000;

        public string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password tidak boleh kosong.", nameof(password));
            }

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, DefaultIterations, HashAlgorithmName.SHA256, KeySize);

            return $"{DefaultIterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public bool Verify(string password, string encodedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(encodedHash))
            {
                return false;
            }

            var parts = encodedHash.Split('.', 3);
            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            {
                return false;
            }

            byte[] salt, expectedKey;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expectedKey = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedKey.Length);

            // Perbandingan waktu-tetap supaya tidak membocorkan informasi lewat timing.
            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
    }
}

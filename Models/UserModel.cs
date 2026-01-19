using System.Security.Cryptography;

namespace CST350_MinesweeperMilestone.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public int Age { get; set; }
        public string State { get; set; }
        public string Email { get; set; }

        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public byte[] Salt { get; set; }

        public void SetPassword(string password)
        {
            // 16 bytes salt
            Salt = RandomNumberGenerator.GetBytes(16);

            // PBKDF2 hash
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                Salt,
                100_000,
                HashAlgorithmName.SHA256
            );

            PasswordHash = Convert.ToBase64String(pbkdf2.GetBytes(32));
        }

        public bool VerifyPassword(string password)
        {
            if (Salt == null || string.IsNullOrEmpty(PasswordHash))
                return false;

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                Salt,
                100_000,
                HashAlgorithmName.SHA256
            );

            var incoming = pbkdf2.GetBytes(32);
            var stored = Convert.FromBase64String(PasswordHash);

            return CryptographicOperations.FixedTimeEquals(stored, incoming);
        }
    }
}

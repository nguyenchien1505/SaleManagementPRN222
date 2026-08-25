using BCrypt.Net;

namespace WebBanHang.Utilities
{
    /// <summary>
    /// Utility class for password hashing and verification using BCrypt
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Generates BCrypt hash for a given password
        /// Usage: var hash = PasswordHasher.HashPassword("123456");
        /// </summary>
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifies if a password matches the hash
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        /// <summary>
        /// Generate hash for default password "123456"
        /// This is useful for seeding test data
        /// </summary>
        public static string GetDefaultPasswordHash()
        {
            // $2a$11$fOEoYn3P/nPfmLvp.3fjKOG4rkGrqfaKG8BhJMDPBt7fJpXXHWCr2 is hash of "123456"
            return "$2a$11$fOEoYn3P/nPfmLvp.3fjKOG4rkGrqfaKG8BhJMDPBt7fJpXXHWCr2";
        }
    }
}

using BCrypt.Net;

namespace WebBanHang
{
    class Program
    {
        static void Main()
        {
            string password = "123456";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Hash: {hash}");

            // Verify immediately
            bool verified = BCrypt.Net.BCrypt.Verify(password, hash);
            Console.WriteLine($"Verified: {verified}");

            Console.WriteLine("\nCopy hash trên vào SQL script:");
            Console.WriteLine($"UPDATE [User] SET PasswordHash = '{hash}' WHERE UserId > 0;");
        }
    }
}

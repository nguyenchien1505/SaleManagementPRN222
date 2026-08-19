using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBanHang.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserPasswords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // BCrypt hash for password "123456"
            // Generated using: BCrypt.Net.BCrypt.HashPassword("123456")
            string passwordHash = "$2a$11$NRt2U.nQ6sFkSr4U8S1jWeBPwhjf2sj8OvBbK8u7EqRPXXp0sTc8K";

            migrationBuilder.Sql(
                $"UPDATE [User] SET PasswordHash = N'{passwordHash}' WHERE UserId > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration cannot be rolled back safely without knowing original passwords
        }
    }
}

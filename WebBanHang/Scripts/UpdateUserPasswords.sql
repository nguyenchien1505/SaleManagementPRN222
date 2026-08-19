-- Script cập nhật mật khẩu cho tất cả users
-- Mật khẩu mặc định: 123456
-- Hash được tạo từ BCrypt.Net.BCrypt.HashPassword("123456")

-- Nếu bạn cần hash mới, chạy C# code này trước:
-- string hash = BCrypt.Net.BCrypt.HashPassword("123456");
-- Console.WriteLine(hash);

-- Sau đó replace hash bên dưới

-- Cách 1: Dùng hash đã kiểm chứng
UPDATE [User]
SET PasswordHash = '$2y$11$R9h7cIPz0gi.URNNX3kh2OPST9/PgBkqquzi.Ss8KqUgO2t0jKMm2'
WHERE UserId > 0;

-- Kiểm tra kết quả
SELECT UserId, Email, FullName, Role, PasswordHash FROM [User];


-- ============================================================
-- Cập nhật constraint CK_Users_Role: cho phép role 'Manager'
-- Chạy trên database WebBanHang (SQL Server)
-- Script idempotent: chạy lại nhiều lần không bị lỗi
-- ============================================================

USE WebBanHang;
GO

-- 1. Xóa constraint cũ (nếu tồn tại)
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Users_Role')
BEGIN
    ALTER TABLE dbo.Users DROP CONSTRAINT CK_Users_Role;
    PRINT N'Da xoa constraint cu CK_Users_Role';
END
GO

-- 2. Tạo lại constraint mới, có thêm 'Manager'
ALTER TABLE dbo.Users WITH CHECK ADD CONSTRAINT CK_Users_Role
    CHECK (([Role] = 'Customer' OR [Role] = 'Sales' OR [Role] = 'Admin' OR [Role] = 'Manager'));
GO

-- 3. Kiểm tra kết quả: definition phải chứa cả 4 role
SELECT name, definition
FROM sys.check_constraints
WHERE name = 'CK_Users_Role';
GO

-- ============================================================
-- (TÙY CHỌN) Tạo nhanh tài khoản Manager để test
-- Bỏ comment đoạn dưới nếu máy chưa có user nào giữ role Manager.
-- Mật khẩu: 123456 (BCrypt hash dùng chung như các tài khoản seed)
-- ============================================================
-- IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Role = 'Manager')
-- BEGIN
--     INSERT INTO dbo.Users (Username, PasswordHash, Email, FullName, Role, IsActive, CreatedDate, IsDeleted)
--     VALUES (
--         N'manager1',
--         N'$2a$11$hK83maCuLc2R0XNfgGJQf.W2ACeKxUTGOwzRs1SNiMPb9n1AeA9wi',
--         N'manager@store.com',
--         N'Quản lý',
--         N'Manager',
--         1,
--         GETDATE(),
--         0);
--     PRINT N'Da tao tai khoan manager1 / 123456';
-- END
-- GO

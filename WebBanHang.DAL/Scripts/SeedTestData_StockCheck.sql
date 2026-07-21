-- =====================================================================
-- SCRIPT TẠO DỮ LIỆU TEST: "Kiểm tra tồn kho"
-- 1) Xem tồn hiện tại + Không cho xác nhận đơn khi Quantity đặt > Quantity tồn
-- 2) Cảnh báo sắp hết hàng khi tồn kho < 10
-- Chạy an toàn nhiều lần: script sẽ xóa dữ liệu test cũ (mã bắt đầu TEST-) trước khi tạo lại.
-- =====================================================================

-- ---- 0. Dọn dữ liệu test cũ (nếu chạy lại script) ----
DELETE od FROM OrderDetails od
    JOIN Orders o ON o.OrderId = od.OrderId
    WHERE o.OrderCode LIKE 'ORD-TEST-%';
DELETE FROM Orders WHERE OrderCode LIKE 'ORD-TEST-%';
DELETE FROM Products WHERE Code LIKE 'TEST-%';
GO

-- ---- 1. Lấy sẵn 1 CategoryId, 1 Admin/Sales UserId hợp lệ trong DB hiện tại ----
DECLARE @CategoryId INT = (SELECT TOP 1 CategoryId FROM Categories ORDER BY CategoryId);
DECLARE @StaffUserId INT = (SELECT TOP 1 UserId FROM Users WHERE Role IN ('Admin', 'Sales') ORDER BY UserId);

IF @CategoryId IS NULL OR @StaffUserId IS NULL
BEGIN
    RAISERROR('Chưa có Category hoặc Admin/Sales User nào trong DB. Vui lòng tạo trước khi chạy script này.', 16, 1);
    RETURN;
END

-- ---- 2. Lấy (hoặc tạo mới) 1 Customer để gắn đơn hàng test ----
DECLARE @CustomerId INT = (SELECT TOP 1 CustomerId FROM Customers ORDER BY CustomerId);

IF @CustomerId IS NULL
BEGIN
    DECLARE @TestCustomerUserId INT;

    INSERT INTO Users (Username, PasswordHash, FullName, Email, Role, IsActive, CreatedDate)
    VALUES ('test_customer_stock', '$2a$11$abcdefghijklmnopqrstuuABCDEFGHIJKLMNOPQRSTUVWXYZ012345', N'Khách hàng Test Tồn Kho', 'test_customer_stock@example.com', 'Customer', 1, GETDATE());

    SET @TestCustomerUserId = SCOPE_IDENTITY();

    INSERT INTO Customers (UserId, Phone, Address, CreatedDate)
    VALUES (@TestCustomerUserId, '0900000000', N'123 Đường Test, Quận 1, TP.HCM', GETDATE());

    SET @CustomerId = SCOPE_IDENTITY();
END

-- ---- 3. Tạo các sản phẩm test ----

-- 3a) Sản phẩm tồn kho THẤP (< 10) -> để test "Cảnh báo sắp hết hàng" ở màn hình Kiểm tra tồn kho
INSERT INTO Products (Code, Name, Description, CategoryId, ImportPrice, SellingPrice, StockQuantity, Status, CreatedBy, CreatedDate)
VALUES
('TEST-LOWSTOCK-05', N'[TEST] Tồn kho thấp - còn 5', N'Dùng để kiểm tra cảnh báo sắp hết hàng (tồn < 10)', @CategoryId, 50000, 80000, 5, 'Active', @StaffUserId, GETDATE()),
('TEST-LOWSTOCK-09', N'[TEST] Tồn kho cận ngưỡng - còn 9', N'Dùng để kiểm tra cảnh báo sắp hết hàng (tồn < 10)', @CategoryId, 50000, 80000, 9, 'Active', @StaffUserId, GETDATE()),
('TEST-INSTOCK-10',  N'[TEST] Đối chứng - còn đúng 10 (KHÔNG cảnh báo)', N'Đối chứng: 10 không nhỏ hơn 10 nên KHÔNG cảnh báo', @CategoryId, 50000, 80000, 10, 'Active', @StaffUserId, GETDATE()),
('TEST-INSTOCK-50',  N'[TEST] Đối chứng - còn nhiều (50)', N'Đối chứng: tồn nhiều, không cảnh báo', @CategoryId, 50000, 80000, 50, 'Active', @StaffUserId, GETDATE());

-- 3b) Sản phẩm dùng để test chặn xác nhận đơn khi đặt vượt tồn kho
INSERT INTO Products (Code, Name, Description, CategoryId, ImportPrice, SellingPrice, StockQuantity, Status, CreatedBy, CreatedDate)
VALUES
('TEST-BLOCK-STOCK3',  N'[TEST] Chỉ còn 3 - đơn đặt 10 (PHẢI BỊ CHẶN khi xác nhận)', N'Test: tồn 3, đơn test đặt 10 > tồn -> xác nhận phải báo lỗi', @CategoryId, 50000, 80000, 3, 'Active', @StaffUserId, GETDATE()),
('TEST-ALLOW-STOCK3',  N'[TEST] Còn 3 - đơn đặt 1 (PHẢI XÁC NHẬN ĐƯỢC)', N'Test: tồn 3, đơn test đặt 1 <= tồn -> xác nhận phải thành công', @CategoryId, 50000, 80000, 3, 'Active', @StaffUserId, GETDATE());

DECLARE @ProductBlockId INT = (SELECT ProductId FROM Products WHERE Code = 'TEST-BLOCK-STOCK3');
DECLARE @ProductAllowId INT = (SELECT ProductId FROM Products WHERE Code = 'TEST-ALLOW-STOCK3');

-- ---- 4. Tạo 2 đơn hàng Draft (trạng thái "chờ xác nhận") để test bấm "Xác nhận" ----

-- 4a) Đơn hàng ĐẶT VƯỢT TỒN KHO: tồn 3, đặt 10 -> Khi bấm "Xác nhận" (Confirmed) PHẢI bị chặn + báo lỗi thiếu tồn kho
DECLARE @OrderCodeBlock NVARCHAR(50) = CONCAT('ORD-TEST-BLOCK-', FORMAT(GETDATE(), 'yyyyMMddHHmmss'));
INSERT INTO Orders (OrderCode, CustomerId, CreatedBy, OrderDate, SubTotal, DiscountAmount, TotalAmount, Status, ShippingAddress, ShippingPhone)
VALUES (@OrderCodeBlock, @CustomerId, @StaffUserId, GETDATE(), 800000, 0, 800000, 'Draft', N'123 Đường Test, Quận 1, TP.HCM', '0900000000');

DECLARE @OrderBlockId INT = SCOPE_IDENTITY();
INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice, Total)
VALUES (@OrderBlockId, @ProductBlockId, 10, 80000, 800000);

-- 4b) Đơn hàng ĐẶT TRONG GIỚI HẠN TỒN KHO: tồn 3, đặt 1 -> Khi bấm "Xác nhận" PHẢI thành công
DECLARE @OrderCodeAllow NVARCHAR(50) = CONCAT('ORD-TEST-ALLOW-', FORMAT(GETDATE(), 'yyyyMMddHHmmss'));
INSERT INTO Orders (OrderCode, CustomerId, CreatedBy, OrderDate, SubTotal, DiscountAmount, TotalAmount, Status, ShippingAddress, ShippingPhone)
VALUES (@OrderCodeAllow, @CustomerId, @StaffUserId, GETDATE(), 80000, 0, 80000, 'Draft', N'123 Đường Test, Quận 1, TP.HCM', '0900000000');

DECLARE @OrderAllowId INT = SCOPE_IDENTITY();
INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice, Total)
VALUES (@OrderAllowId, @ProductAllowId, 1, 80000, 80000);

-- ---- 5. In kết quả để đối chiếu ----
SELECT Code, Name, StockQuantity,
       CASE WHEN StockQuantity < 10 THEN N'⚠ SẮP HẾT HÀNG (đúng như mong đợi)' ELSE N'Còn hàng (đối chứng)' END AS KetQuaMongDoi
FROM Products WHERE Code LIKE 'TEST-%'
ORDER BY Code;

SELECT o.OrderCode, o.Status, od.Quantity AS SoLuongDat, p.StockQuantity AS TonKhoHienTai,
       CASE WHEN od.Quantity > p.StockQuantity THEN N'Khi xác nhận PHẢI báo lỗi thiếu tồn kho'
            ELSE N'Khi xác nhận PHẢI thành công' END AS KetQuaMongDoi
FROM Orders o
JOIN OrderDetails od ON od.OrderId = o.OrderId
JOIN Products p ON p.ProductId = od.ProductId
WHERE o.OrderCode LIKE 'ORD-TEST-%';
GO

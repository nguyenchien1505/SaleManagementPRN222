# Tổng Quan Dự Án WebBanHang

## 1. Giới thiệu

`WebBanHang` là hệ thống thương mại điện tử được xây dựng bằng ASP.NET Core MVC trên nền tảng .NET 8. Hệ thống hỗ trợ quản lý sản phẩm, danh mục, người dùng, kho hàng, đơn hàng, khuyến mãi và thanh toán trực tuyến.

Solution gồm ba project chính:

- `WebBanHang`: tầng giao diện web và xử lý request HTTP.
- `WebBanHang.BLL`: tầng xử lý nghiệp vụ.
- `WebBanHang.DAL`: tầng truy cập dữ liệu và mô hình cơ sở dữ liệu.

## 2. Công nghệ sử dụng

- .NET 8
- ASP.NET Core MVC
- Entity Framework Core 8
- SQL Server
- Razor View
- Dependency Injection
- Cookie Authentication
- Google Authentication
- Session
- BCrypt.Net để băm mật khẩu
- EPPlus để import/export Excel
- VNPay Sandbox cho thanh toán trực tuyến
- SMTP Gmail cho chức năng gửi email

## 3. Kiến trúc tổng thể

```text
Người dùng
    |
    v
ASP.NET Core MVC - WebBanHang
    |
    v
BLL - Business Logic Layer
    |
    v
DAL - Data Access Layer
    |
    v
SQL Server
```

### 3.1. WebBanHang

Tầng Web chịu trách nhiệm nhận request, hiển thị Razor View, phân chia chức năng theo MVC Area, kiểm tra session và quyền truy cập, gọi service từ BLL, đồng thời phục vụ CSS, JavaScript và hình ảnh.

Các khu vực chính:

```text
Areas/
├── Admin/
├── Customer/
└── Sale/
```

### 3.2. WebBanHang.BLL

BLL chứa logic nghiệp vụ và giao tiếp với DAL thông qua các interface repository.

Các service chính:

- `AuthService`: xem và cập nhật thông tin cá nhân.
- `UserService`: đăng nhập, đăng ký, quản lý người dùng, Google Login và đặt lại mật khẩu.
- `ProductService`: quản lý sản phẩm, hình ảnh, xóa mềm và import/export Excel.
- `CategoryService`: quản lý danh mục sản phẩm.
- `OrderService`: checkout, tạo đơn, xem đơn và cập nhật trạng thái.
- `CartService`: quản lý giỏ hàng.
- `PromotionService`: quản lý và kiểm tra mã khuyến mãi.
- `InventoryService`: nhập kho và quản lý giao dịch tồn kho.
- `DashboardService`: tổng hợp thống kê kinh doanh.
- `CustomerService`: quản lý khách hàng.
- `AuditLogService`: ghi nhận lịch sử thao tác.
- `EmailService`: gửi email.
- `VNPayLibrary`: hỗ trợ tạo và xác thực thanh toán VNPay.

### 3.3. WebBanHang.DAL

DAL chịu trách nhiệm kết nối SQL Server, khai báo entity, cấu hình quan hệ, truy vấn dữ liệu, cung cấp repository và quản lý migration/interceptor.

```text
DAL/
├── Abstractions/
├── Context/
├── Entities/
├── Interceptors/
├── Migrations/
├── Models/
├── QueryModels/
└── Repositories/
```

## 4. Các chức năng chính

### 4.1. Customer

- Xem, tìm kiếm, lọc, sắp xếp và phân trang sản phẩm.
- Xem chi tiết sản phẩm.
- Thêm, sửa số lượng và xóa sản phẩm khỏi giỏ hàng.
- Nhập và kiểm tra mã khuyến mãi.
- Checkout một hoặc nhiều sản phẩm.
- Thanh toán COD hoặc VNPay.
- Xem lịch sử và hủy đơn hàng trong phạm vi cho phép.
- Xem và cập nhật hồ sơ cá nhân.

Các controller chính:

```text
Areas/Customer/Controllers/
├── AccountController.cs
├── CartController.cs
├── HomeController.cs
├── OrderController.cs
├── PaymentController.cs
└── ProductController.cs
```

### 4.2. Admin

- Xem dashboard và thống kê.
- Quản lý người dùng, sản phẩm, hình ảnh, danh mục và khuyến mãi.
- Quản lý tồn kho và nhập hàng.
- Quản lý đơn hàng.
- Xem audit log.
- Import/export dữ liệu bằng Excel.
- Xóa mềm, khôi phục hoặc xóa vĩnh viễn dữ liệu.

Các controller chính:

```text
Areas/Admin/Controllers/
├── AuditLogController.cs
├── CategoryController.cs
├── DashboardController.cs
├── InventoryController.cs
├── ProductController.cs
├── PromotionController.cs
└── UsersController.cs
```

### 4.3. Sale

- Xem dashboard bán hàng.
- Xem, tìm kiếm và lọc đơn hàng.
- Xem chi tiết đơn hàng.
- Cập nhật trạng thái đơn hàng.
- Xem thông tin sản phẩm.

Các controller chính:

```text
Areas/Sale/Controllers/
├── DashBoardController.cs
├── OrderController.cs
└── ProductController.cs
```

## 5. Mô hình dữ liệu

Các entity chính:

- `User`: tài khoản và thông tin người dùng.
- `Category`: danh mục sản phẩm, hỗ trợ danh mục cha/con.
- `Product`: sản phẩm, giá nhập, giá bán và tồn kho.
- `ProductImage`: hình ảnh sản phẩm.
- `Cart`, `CartItem`: giỏ hàng và sản phẩm trong giỏ.
- `Order`, `OrderDetail`: đơn hàng và chi tiết đơn hàng.
- `Promotion`, `OrderPromotion`: mã giảm giá và khuyến mãi đã áp dụng.
- `InventoryTransaction`: lịch sử giao dịch tồn kho.
- `AuditLog`: lịch sử thao tác trong hệ thống.

Quan hệ dữ liệu tiêu biểu:

```text
User 1 ─── n Cart
Cart 1 ─── n CartItem
Product 1 ─── n CartItem
Category 1 ─── n Product
Product 1 ─── n ProductImage
User 1 ─── n Order
Order 1 ─── n OrderDetail
Product 1 ─── n OrderDetail
Order n ─── n Promotion
Product 1 ─── n InventoryTransaction
```

## 6. Xóa mềm và xóa cứng

Hệ thống sử dụng query filter của Entity Framework Core để ẩn dữ liệu đã xóa trong các truy vấn thông thường:

- Product: `Status = "Deleted"`.
- Category: `Status = "Deleted"`.
- User: `IsDeleted = true`.

Admin có thể xem dữ liệu đã xóa, khôi phục hoặc xóa cứng khi cần.

## 7. Luồng đặt hàng

```text
Khách hàng xem sản phẩm
        |
        v
Thêm sản phẩm vào giỏ hàng
        |
        v
Chọn sản phẩm và mã khuyến mãi
        |
        v
Checkout
        |
        v
Kiểm tra tồn kho và khuyến mãi
        |
        v
Tạo Order và OrderDetail
        |
        +--> COD: ghi nhận trạng thái chờ thanh toán
        |
        +--> VNPay: chuyển sang cổng thanh toán
        |
        v
Cập nhật trạng thái đơn hàng
```

## 8. Authentication và phân quyền

Hệ thống hỗ trợ đăng nhập nội bộ, đăng ký, đăng nhập Google, Cookie Authentication và session. Vai trò người dùng được lưu trong session và được kiểm tra bằng bộ lọc `RoleAuthorize`.

Các request POST quan trọng sử dụng `[ValidateAntiForgeryToken]` để giảm nguy cơ CSRF.

## 9. Cấu hình và cách chạy

Cấu hình nằm tại:

```text
WebBanHang/appsettings.json
WebBanHang/appsettings.Development.json
```

Yêu cầu môi trường:

- .NET 8 SDK.
- SQL Server hoặc SQL Server Express.
- Cơ sở dữ liệu phù hợp với connection string.
- Cấu hình Google, SMTP và VNPay nếu sử dụng các chức năng tương ứng.

Các lệnh cơ bản:

```bash
dotnet restore
dotnet build
dotnet ef database update --project WebBanHang.DAL --startup-project WebBanHang
dotnet run --project WebBanHang
```

URL chạy ứng dụng được cấu hình trong `WebBanHang/Properties/launchSettings.json`.

## 10. Đánh giá tổng quan

### Ưu điểm

- Kiến trúc được chia thành Web, BLL và DAL.
- Sử dụng Dependency Injection, service interface và repository interface.
- Hỗ trợ nhiều vai trò người dùng.
- Bao phủ các nghiệp vụ chính của hệ thống bán hàng.
- Có xóa mềm, khôi phục và audit log.
- Có import/export Excel.
- Hỗ trợ COD, VNPay, Google Login và gửi email.

### Điểm cần cải thiện

- Không nên lưu Google Secret, SMTP Password, VNPay Hash Secret và thông tin nhạy cảm trong `appsettings.json`.
- Nên chuyển secret sang .NET User Secrets, biến môi trường hoặc secret manager.
- Cần thu hồi và tạo lại các secret đã từng bị công khai.
- Nên bổ sung test tự động cho checkout, tồn kho, khuyến mãi và cập nhật trạng thái đơn.
- Nên kiểm tra quyền sở hữu đơn hàng khi khách hàng hủy hoặc cập nhật đơn.
- Nên bổ sung logging và xử lý lỗi tập trung cho môi trường production.
- Nên kiểm tra đồng nhất phiên bản Entity Framework Core giữa các project.
- Nên bổ sung tài liệu database hoặc sơ đồ ERD khi bàn giao dự án.

## 11. Kết luận

`WebBanHang` là hệ thống bán hàng trực tuyến được xây dựng theo mô hình ASP.NET Core MVC nhiều tầng. Dự án hỗ trợ quản lý sản phẩm, khách hàng, giỏ hàng, đơn hàng, kho, khuyến mãi, thanh toán và báo cáo.

Kiến trúc hiện tại phù hợp cho đồ án hoặc hệ thống quy mô vừa. Trước khi triển khai thực tế, cần ưu tiên xử lý bảo mật cấu hình, bổ sung kiểm thử tự động và hoàn thiện kiểm soát quyền truy cập.

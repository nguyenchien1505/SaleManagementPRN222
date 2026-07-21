using System;

namespace WebBanHang.DAL.Entities;

// Lưu lịch sử: Ai tạo sản phẩm / Ai sửa sản phẩm / Ai xác nhận đơn hàng
public partial class AuditLog
{
    public int AuditLogId { get; set; }

    // Tên bảng/đối tượng bị tác động, vd: "Product", "Order"
    public string EntityName { get; set; } = null!;

    // Id của bản ghi bị tác động (ProductId, OrderId, ...)
    public int EntityId { get; set; }

    // Hành động: "Create", "Update", "Confirm", "Cancel", ...
    public string Action { get; set; } = null!;

    // Người thực hiện hành động
    public int PerformedBy { get; set; }

    public string? PerformedByName { get; set; }

    // Mô tả chi tiết (vd: "Tạo sản phẩm ABC", "Xác nhận đơn hàng #ORD-...")
    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
}

using System;

namespace WebBanHang.DAL.Entities
{
    public class InventoryTransaction
    {
        public int TransactionId { get; set; } // Khóa chính (PK__Inventor__55433A6B3457E96D)

        public int ProductId { get; set; } // Khóa ngoại kết nối bảng Product

        public string Type { get; set; } = string.Empty; // Loại biến động: "In", "Out"

        public int Quantity { get; set; } // Số lượng

        public DateTime? CreatedDate { get; set; }

        public string? Note { get; set; } // Ghi chú (Ví dụ: Nhập kho hệ thống, Xuất kho theo đơn)

        public int CreatedBy { get; set; } // Khóa ngoại kết nối bảng User (Người thực hiện)

        // Các thuộc tính liên kết dữ liệu (Navigation properties)
        public virtual User CreatedByNavigation { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
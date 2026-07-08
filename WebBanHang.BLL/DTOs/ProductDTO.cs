using System; // Đảm bảo có thư viện này để dùng DateTime
using System.Collections.Generic;

namespace WebBanHang.BLL.DTOs
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? Status { get; set; }
        public int CreatedBy { get; set; }

        // Thêm dòng này vào class của bạn:
        public DateTime? CreatedDate { get; set; }

        public IEnumerable<ProductImageDTO>? Images { get; set; }
    }
}
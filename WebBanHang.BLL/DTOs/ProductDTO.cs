using System; // Đảm bảo có thư viện này để dùng DateTime
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;


namespace WebBanHang.BLL.DTOs
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public decimal ImportPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? Status { get; set; }
        public string? SupplierName { get; set; }
        public int CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public IEnumerable<ProductImageDTO>? Images { get; set; }
    }
}
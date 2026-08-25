using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.BLL.DTOs
{
    public class DetailProductDTO
    {
        public int ProductId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal ImportPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? SupplierName { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<ProductImageDTO>? ExistingImages { get; set; }
    }
}

using WebBanHang.BLL.DTOs;

namespace WebBanHang.ViewModels
{
    public class ProductDetailVM
    {
        public int ProductId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal SellingPrice { get; set; }
        public List<ProductImageDTO> Images { get; set; }
        public int StockQuantity { get; set; }
        public string? SupplierName { get; set; }
        public string Status { get; set; } 
        public DateTime? CreatedDate { get; set; }

        public List<CategoryDTO> Categories { get; set; }


    }
}

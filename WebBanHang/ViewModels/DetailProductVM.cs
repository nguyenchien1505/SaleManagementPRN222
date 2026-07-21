using WebBanHang.BLL.DTOs;

namespace WebBanHang.ViewModels
{
    public class DetailProductVM
    {
        public int ProductId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal ImportPrice { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<ProductImageDTO>? ExistingImages { get; set; }

    }
}

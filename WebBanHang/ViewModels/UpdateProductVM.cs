using System.ComponentModel.DataAnnotations;
using WebBanHang.BLL.DTOs;

namespace WebBanHang.ViewModels
{
    public class UpdateProductVM
    {
        public int Id { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public List<IFormFile>? ImageFiles { get; set; }
        [Required]
        public decimal SellingPrice { get; set; }
        [Required]
        public decimal ImportPrice { get; set; }
        [Required]
        public int StockQuantity { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public string Description { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
        public List<CategoryDTO> Categories { get; set; } = new();
        public int CategoryId { get; set; }
        public List<ProductImageDTO> ExistingImages { get; set; } = new();
        public DateTime? CreatedDate { get; set; }
        public int CreatedBy { get; set; }
    }
}

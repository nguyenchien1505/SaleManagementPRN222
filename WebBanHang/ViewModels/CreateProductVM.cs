using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace WebBanHang.ViewModels
{
    public class CreateProductVM
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
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public List<CategoryVM> Categories { get; set; } = new();
    }
}

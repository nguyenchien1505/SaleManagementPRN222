using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using WebBanHang.BLL.DTOs;
namespace WebBanHang.ViewModels
{
    public class UpdateProductVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        [StringLength(50, ErrorMessage = "Mã sản phẩm không được vượt quá 50 ký tự")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá nhập phải lớn hơn hoặc bằng 0")]
        public decimal ImportPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
        public decimal SellingPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải lớn hơn hoặc bằng 0")]
        public int StockQuantity { get; set; }

        public string? Description { get; set; }
        public string? SupplierName { get; set; }
        public string Status { get; set; }

        public List<ProductImageDTO> ExistingImages { get; set; } = new();
        public DateTime? CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public string? CategoryName { get; set; }
        public IFormFile? ImageFile { get; set; }

        public List<CategoryDTO>? Categories { get; set; }

    }
}
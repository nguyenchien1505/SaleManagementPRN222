using System.ComponentModel.DataAnnotations;

namespace WebBanHang.ViewModels
{
    public class CreateCategoryVM 
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Mô tả không được để trống")]
        [StringLength(50, ErrorMessage = "Mô tả không được vượt quá 50 ký tự")]
        public string Description { get; set; }
        public string Status { get; set; }

    }
}

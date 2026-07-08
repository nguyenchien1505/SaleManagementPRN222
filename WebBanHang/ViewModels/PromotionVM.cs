using System;
using System.ComponentModel.DataAnnotations;

namespace WebBanHang.ViewModels
{
    public class PromotionVM
    {
        public int PromotionId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã giảm giá.")]
        [StringLength(50, ErrorMessage = "Mã không quá 50 ký tự.")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn loại giảm giá.")]
        public string DiscountType { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập giá trị.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải từ 0 trở lên.")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá trị đơn tối thiểu.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải từ 0 trở lên.")]
        public decimal MinOrderValue { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);

        [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
        public string Status { get; set; } = "Active";
    }
}
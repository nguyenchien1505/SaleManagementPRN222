using System.ComponentModel.DataAnnotations;

namespace WebBanHang.ViewModels
{
    public class SettingVM
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        // Đổi mật khẩu (tùy chọn - để trống nếu không muốn đổi)
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string? ConfirmPassword { get; set; }
    }
}

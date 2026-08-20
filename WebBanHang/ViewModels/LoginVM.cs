using System.ComponentModel.DataAnnotations;

namespace WebBanHang.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Vui long nhap ten dang nhap")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui long nhap mat khau")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace WebBanHang.ViewModels
{
    public class CreateUserVM
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
        public bool IsActive { get; set; }

        public string? VerificationPassword { get; set; }
    }
}

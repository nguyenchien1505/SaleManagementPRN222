using System.ComponentModel.DataAnnotations;

namespace WebBanHang.ViewModels
{
    public class UpdateUserVM
    {
        public int UserId { get; set; }
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Role { get; set; }
        //public string Phone{ get; set; }
        public bool IsActive { get; set; }
        public string? VerificationPassword { get; set; }

    }
}

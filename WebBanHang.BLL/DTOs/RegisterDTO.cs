using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WebBanHang.BLL.DTOs
{
    public class RegisterDTO
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public string Password { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.BLL.DTOs
{
    public class UpdateUserDTO
    {
        public int Id { get; set; }
        public string? Password { get; set; }
        public string Email { get; set; } 
        public string FullName { get; set; } 
        public string Role { get; set; } 
        public bool IsActive { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.BLL.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string Role { get; set; }
        public bool IsActived { get; set; }
        public string? Avatar { get; set; }

    }
}

using WebBanHang.BLL.DTOs;
using WebBanHang.DAL.Entities;

namespace WebBanHang.ViewModels
{
    public class UserManagementVM 
    {
        public IEnumerable<UserDTO> Users { get; set; }
        
        public string SearchTerm { get; set; }

        public string RoleFilter { get; set; }

        public int TotalUsers { get; set; }

    }
}

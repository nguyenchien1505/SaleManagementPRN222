using WebBanHang.BLL.DTOs;
using WebBanHang.DAL.Entities;

namespace WebBanHang.ViewModels
{
    public class UserManagementVM 
    {
        public IEnumerable<UserDTO> Users { get; set; }
        public string SearchInput { get; set; }
        public string RoleFilter { get; set; }
        public int SortStatus { get; set; } = 0;
        public int TotalUsers { get; set; }

    }
}

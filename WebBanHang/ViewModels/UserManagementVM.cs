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

        // Pagination
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; } = 1;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

    }
}

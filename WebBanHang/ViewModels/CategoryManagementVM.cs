using WebBanHang.BLL.DTOs;

namespace WebBanHang.ViewModels
{
    public class CategoryManagementVM
    {
        public IEnumerable<CategoryDTO> Categories { get; set; }
        public string SearchInput { get; set; }
        public string StatusFilter { get; set; } 
        public int SortStatus { get; set; } = 0;

    }
}

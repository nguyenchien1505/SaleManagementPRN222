using WebBanHang.BLL.DTOs;

namespace WebBanHang.ViewModels
{
    public class ProductManagementVM
    {
        public IEnumerable<ProductDTO> Products { get; set; }
        public int Count() { return Products.Count(); }
        public string SearchInput { get; set; }
        public string CateFilter { get; set; }
        public string StatusFilter { get; set; }
        public int SortStatus { get; set; } = 0;
        public string SortColumn { get; set; } = "";
        public List<CategoryVM> Categories { get; set; }
    }
}

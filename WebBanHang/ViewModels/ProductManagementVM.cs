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

        // Pagination
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; } = 1;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}

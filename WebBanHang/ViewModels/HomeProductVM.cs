using WebBanHang.BLL.DTOs;

namespace WebBanHang.ViewModels
{
    public class HomeProductVM
    {
        public IEnumerable<ProductDTO> Products { get; set; } = new List<ProductDTO>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? SearchString { get; set; }
        public string? SortOrder { get; set; }
        public int? CategoryId { get; set; }
        public IEnumerable<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
    }
}

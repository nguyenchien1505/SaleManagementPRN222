using WebBanHang.BLL.DTOs;

namespace WebBanHang.ViewModels
{
    public class ProductManagementVM
    {
        public IEnumerable<ProductDTO> Products { get; set; }
        public int Count() { return Products.Count(); }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IProductService
    {
        Task<bool> CreateProductAsync(ProductDTO dto);
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO> GetProductByIdAsync(int id);
        //Admin
        Task<DetailProductDTO> GetDetailProductByIdAsync(int id);
        Task<IEnumerable<ProductDTO>> GetAllProductsIncludeDeleteAsync();
        Task<ProductDTO> GetProductByIdIncludeDeleteAsync(int id);
        Task<DetailProductDTO> GetDetailProductByIdIncludeDeleteAsync(int id);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateProductAsync(ProductDTO dto);
        Task<bool> RestoreProductAsync(int id);
        Task<bool> HardDeleteProductAsync(int id);

    }
}
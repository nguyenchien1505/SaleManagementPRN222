using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IProductService
    {
        Task<bool> CreateProductAsync(ProductDTO dto);
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO> GetProductByCodeAsync(string code);
        Task<ProductDTO> GetProductByIdAsync(int id);

        Task<DetailProductDTO> GetDetailProductByIdAsync(int id);
        Task<bool> DeleteProductAsync(int id);

        Task<bool> UpdateProductAsync(ProductDTO dto);

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO> GetProductByCodeAsync(string code);
        Task<bool> CreateProductAsync(ProductDTO dto);
        Task<bool> UpdateProductAsync(ProductDTO dto);
        Task<ProductDTO> GetProductByIdAsync(int id);
        Task<DetailProductDTO> GetDetailProductByIdAsync(int id);
        Task<bool> DeleteProductAsync(int id);
    }
}

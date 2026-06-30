using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product?> GetProductDetailsAsync(int id);
        Task<IEnumerable<Product>> GetPagedProductsAsync(int pageNumber, int pageSize, string searchString, string sortOrder, int? categoryId = null);
        Task<int> GetTotalProductCountAsync(string searchString, int? categoryId = null);
        Task<bool> AddProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> AddProductImageAsync(int productId, string imageUrl, bool isPrimary);
        Task<bool> RemoveProductImagesAsync(int productId);
        Task<IEnumerable<Product>> GetRelatedProductsAsync(int categoryId, int currentProductId, int count);
    }
}

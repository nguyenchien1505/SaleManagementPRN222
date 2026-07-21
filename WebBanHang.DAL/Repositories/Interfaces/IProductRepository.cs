using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.DAL.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync();

        Task<Product?> GetByIdAsync(int id);

        Task<Product?> GetByCodeAsync(string code);

        Task CreateAsync(Product product);

        Task UpdateAsync(Product product);

        Task<bool> DeleteAsync(int id);
        //Admin
        Task<List<Product>> GetAllIncludeDeleteAsync();

        Task<Product?> GetByIdIncludeDeleteAsync(int id);

        Task<Product?> GetByCodeIncludeDeleteAsync(string code);
        Task<bool> HasHistoricalReferencesAsync(int productId);

        Task<bool> HardDeleteAsync(Product product);


        // Dùng cho InventoryController (đồng bộ, không async)
        List<Product> GetAll();

        Product? GetById(int id);

        List<Product> Search(string keyword);

        List<Product> GetByCategory(int categoryId);
    }

}
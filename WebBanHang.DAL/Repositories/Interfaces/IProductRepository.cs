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


        // Dùng cho InventoryController (đồng bộ, không async)
        List<Product> GetAll();

        Product? GetById(int id);

        List<Product> Search(string keyword);

        List<Product> GetByCategory(int categoryId);
    }

}
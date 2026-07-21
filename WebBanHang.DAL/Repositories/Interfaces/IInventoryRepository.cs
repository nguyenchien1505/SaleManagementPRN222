using System.Collections.Generic;
using WebBanHang.DAL.Entities;

namespace WebBanHang.DAL.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        void AddTransaction(InventoryTransaction transaction);

        List<InventoryTransaction> GetTransactionsByProduct(int productId);

        List<InventoryTransaction> GetAll();

        Product? GetProductWithStock(int productId);

        List<Product> GetLowStockProducts(int threshold);

        void UpdateProductStock(Product product);
    }
}
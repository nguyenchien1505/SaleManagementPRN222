using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebBanHang.DAL.Context;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.DAL.Repositories.Implementations
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly WebBanHangContext _context;

        public InventoryRepository(WebBanHangContext context)
        {
            _context = context;
        }

        public void AddTransaction(InventoryTransaction transaction)
        {
            _context.InventoryTransactions.Add(transaction);
            _context.SaveChanges();
        }

        public List<InventoryTransaction> GetTransactionsByProduct(int productId)
        {
            return _context.InventoryTransactions
                .Include(x => x.Product)
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.CreatedDate)
                .ToList();
        }

        public List<InventoryTransaction> GetAll()
        {
            return _context.InventoryTransactions
                .Include(x => x.Product)
                .OrderByDescending(x => x.CreatedDate)
                .ToList();
        }

        public Product? GetProductWithStock(int productId)
        {
            return _context.Products.Find(productId);
        }

        public List<Product> GetLowStockProducts(int threshold)
        {
            return _context.Products
                .Where(p => p.StockQuantity < threshold)
                .ToList();
        }

        public void UpdateProductStock(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();
        }
    }
}
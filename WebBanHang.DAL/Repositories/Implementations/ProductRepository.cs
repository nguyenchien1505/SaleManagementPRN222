using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Context;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.DAL.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly WebBanHangContext _context;
        public ProductRepository(WebBanHangContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Product product)
        {
            await _context.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;
            product.Status = "Deleted";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products.Include(x => x.Category)
                                          .Include(x => x.ProductImages)
                                          .Include(x => x.CreatedByNavigation)
                                          .Include(x => x.InventoryTransactions)
                                          .Include(x => x.OrderDetails)
                                          .Where(x => !x.Status.Contains("Deleted") && !x.Category.Status.Contains("Deleted")).ToListAsync();
        }

        public List<Product> GetByCategory(int categoryId)
        {
            throw new NotImplementedException();
        }

        public async Task<Product> GetByCodeAsync(string code)
        {
            return await _context.Products.Include(x => x.Category)
                                          .Where(x => x.Code == code && !x.Status.Contains("Deleted") && !x.Category.Status.Contains("Deleted"))
                                          .FirstOrDefaultAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.Include(x => x.Category)
                                          .Include(x => x.ProductImages)
                                          .Include(x => x.CreatedByNavigation)
                                          .Include(x => x.InventoryTransactions)
                                          .Include(x => x.OrderDetails)
                                          .Where(x => x.ProductId == id && !x.Category.Status.Contains("Deleted") && !x.Status.Contains("Deleted"))
                                          .FirstOrDefaultAsync();
        }

        public List<Product> Search(string keyword)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(Product product)
        {
            var existingProduct = await _context.Products.Include(x => x.Category)
                                          .Include(x => x.ProductImages)
                                          .Include(x => x.CreatedByNavigation)
                                          .Include(x => x.InventoryTransactions)
                                          .Include(x => x.OrderDetails)
                                          .Where(x => !x.Category.Status.Contains("Deleted") && !x.Status.Contains("Deleted"))
                                          .FirstOrDefaultAsync(x => x.ProductId == product.ProductId);

            if (existingProduct == null)
            {
                throw new Exception("Không tìm thấy sản phẩm cần cập nhật.");
            }

            existingProduct.Code = product.Code;
            existingProduct.Name = product.Name;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.ImportPrice = product.ImportPrice;
            existingProduct.Description = product.Description;
            existingProduct.SellingPrice = product.SellingPrice;
            existingProduct.ProductImages = product.ProductImages;
            existingProduct.Status = product.Status;
            existingProduct.StockQuantity = product.StockQuantity;

            await _context.SaveChangesAsync();
        }


    }
}

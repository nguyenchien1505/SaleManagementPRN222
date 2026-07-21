using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products.Include(x => x.Category)
                                          .Include(x => x.ProductImages)
                                          .Include(x => x.CreatedByNavigation)
                                          .Include(x => x.InventoryTransactions)
                                          .Include(x => x.OrderDetails)
                                          .ToListAsync();

        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products.Include(x => x.Category)
                                         .Include(x => x.ProductImages)
                                         .Include(x => x.CreatedByNavigation)
                                         .Include(x => x.InventoryTransactions)
                                         .Include(x => x.OrderDetails)
                                         .FirstOrDefaultAsync(x => x.ProductId == id);
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            return await _context.Products.Include(x => x.Category)
                                          .FirstOrDefaultAsync(x => x.Code == code);
        }

        //Admin
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
        public async Task UpdateAsync(Product product)
        {
            var existingProduct = await _context.Products.IgnoreQueryFilters().Include(x => x.Category)
                                          .Include(x => x.ProductImages)
                                          .Include(x => x.CreatedByNavigation)
                                          .Include(x => x.InventoryTransactions)
                                          .Include(x => x.OrderDetails)
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


        public async Task<List<Product>> GetAllIncludeDeleteAsync()
        {
            return await _context.Products.IgnoreQueryFilters()
                                          .Include(x => x.Category)
                                          .Include(x => x.ProductImages)
                                          .Include(x => x.CreatedByNavigation)
                                          .Include(x => x.InventoryTransactions)
                                          .Include(x => x.OrderDetails)
                                          .ToListAsync();
        }

        public async Task<Product?> GetByIdIncludeDeleteAsync(int id)
        {
            return await _context.Products.IgnoreQueryFilters()
                                          .Include(x => x.Category)
                                          .Include(x => x.ProductImages)
                                          .Include(x => x.CreatedByNavigation)
                                          .Include(x => x.InventoryTransactions)
                                          .Include(x => x.OrderDetails)
                                          .FirstOrDefaultAsync(x => x.ProductId == id);
        }

        public async Task<Product?> GetByCodeIncludeDeleteAsync(string code)
        {
            return await _context.Products.IgnoreQueryFilters().Include(x => x.Category) .FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<bool> HasHistoricalReferencesAsync(int productId)
        {
            var hasOrderDetails = await _context.OrderDetails.AnyAsync(od => od.ProductId == productId);

            var hasInventoryTransactions =
                await _context.InventoryTransactions.AnyAsync(x => x.ProductId == productId);

            return hasOrderDetails || hasInventoryTransactions;
        }

        public async Task<bool> HardDeleteAsync(Product product)
        {
            _context.Products.Remove(product);  

            return await _context.SaveChangesAsync() > 0;
        }



        // ---- Sync methods (dùng cho InventoryController) ----
        public List<Product> GetAll()
        {
            return _context.Products
                .Include(p => p.Category)
                .ToList();
        }

        public Product? GetById(int id)
        {
            return _context.Products.Find(id);
        }

        public List<Product> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAll();

            return _context.Products
                .Where(p => p.Name.Contains(keyword) || p.Code.Contains(keyword))
                .ToList();
        }

        public List<Product> GetByCategory(int categoryId)
        {
            return _context.Products.Where(p => p.CategoryId == categoryId)
                .ToList();
        }

        
    }
}
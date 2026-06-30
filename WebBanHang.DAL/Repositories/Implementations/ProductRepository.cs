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

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products.Include(x => x.Category)
                                          .Include(x => x.ProductImages)
                                          .Include(x => x.CreatedByNavigation)
                                          .Include(x => x.InventoryTransactions)
                                          .Include(x => x.OrderDetails).ToListAsync();
        }

        public List<Product> GetByCategory(int categoryId)
        {
            throw new NotImplementedException();
        }

        public async Task<Product> GetByCodeAsync(string code)
        {
            return await _context.Products.Include(x => x.Category).Where(x => x.Code == code).FirstOrDefaultAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.Include(x => x.Category)
                                          .Include(x => x.ProductImages)
                                          .Include(x => x.CreatedByNavigation)
                                          .Include(x => x.InventoryTransactions)
                                          .Include(x => x.OrderDetails).Where(x => x.ProductId == id).FirstOrDefaultAsync();
        }

        public List<Product> Search(string keyword)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}

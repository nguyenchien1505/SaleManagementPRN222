using Microsoft.EntityFrameworkCore; // Bắt buộc phải có để dùng ToListAsync(), FindAsync(),...
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Bắt buộc phải có để dùng Task
using WebBanHang.DAL.Context;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.DAL.Repositories.Implementations
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly WebBanHangContext _context;

        public CategoryRepository(WebBanHangContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            var existingCategory = await _context.Categories.FindAsync(category.CategoryId);
            if (existingCategory == null)
            {
                throw new Exception("Không tìm thấy mã sản phẩm cần cập nhật.");
            }
            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.Status = category.Status;
            existingCategory.Products = category.Products;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = _context.Categories.Find(id);
            category.Status = "Deleted";
            await _context.SaveChangesAsync();

        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories.FirstOrDefaultAsync(x => x.CategoryId == id);
        }

        public async Task<Category> GetByNameAsync(string name)
        {
            return await _context.Categories.FirstOrDefaultAsync(x => x.Name == name);
        }

        //Admin
        public async Task<IEnumerable<Category>> GetAllIncludeDeleteAsync()
        {
            return await _context.Categories.IgnoreQueryFilters().ToListAsync();
        }
        public async Task<Category> GetByIdIncludeDeleteAsync(int id)
        {
            return await _context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.CategoryId == id);
        }

        public async Task<Category> GetByNameIncludeDeleteAsync(string name)
        {
            return await _context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products
                .IgnoreQueryFilters()
                .AnyAsync(p => p.CategoryId == categoryId);
        }

        public async Task<bool> HasChildCategoriesAsync(int categoryId)
        {
            return await _context.Categories
                .IgnoreQueryFilters()
                .AnyAsync(c => c.ParentId == categoryId);
        }

        public async Task HardDeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

    }
}
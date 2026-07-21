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

        // 1. Lấy tất cả danh mục (Async)
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.Where(x => !x.Status.Contains("Deleted")).ToListAsync();
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
            return await _context.Categories.Where(x => x.CategoryId == id && !x.Status.Contains("Deleted")).FirstOrDefaultAsync();
        }

        public async Task<Category> GetByNameAsync(string name)
        {
            return await _context.Categories.Where(x => x.Name == name && !x.Status.Contains("Deleted")).FirstOrDefaultAsync();
        }

    }
}
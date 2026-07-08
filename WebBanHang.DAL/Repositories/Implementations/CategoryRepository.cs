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
            return await _context.Categories.ToListAsync();
        }

        // 2. Tìm danh mục theo Id (Async)
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        // 3. Tìm danh mục theo Tên (Async) - Hàm này để giải quyết lỗi CS1061 ở Service của bạn
        public async Task<Category?> GetByNameAsync(string name)
        {
            // Giả định thực thể Category của bạn có thuộc tính tên là 'Name' hoặc 'CategoryName'
            // Bạn có thể sửa lại 'c.Name' cho đúng với thuộc tính trong DB của bạn
            return await _context.Categories.FirstOrDefaultAsync(c => c.Name == name);
        }

        // 4. Thêm danh mục mới (Async)
        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        // 5. Cập nhật danh mục (Async)
        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        // 6. Xóa danh mục (Async)
        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}
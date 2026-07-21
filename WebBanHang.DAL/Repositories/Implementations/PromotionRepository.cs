using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebBanHang.DAL.Context;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.DAL.Repositories.Implementations
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly WebBanHangContext _context;

        public PromotionRepository(WebBanHangContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Promotion>> GetAllAsync(string? statusFilter, string? searchString)
        {
            var query = _context.Promotions.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Code.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(p => p.Status == statusFilter);
            }

            return await query.ToListAsync();
        }

        public async Task<Promotion?> GetByIdAsync(int id)
        {
            return await _context.Promotions.FindAsync(id);
        }

        public async Task<bool> AddAsync(Promotion promotion)
        {
            await _context.Promotions.AddAsync(promotion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Promotion promotion)
        {
            _context.Promotions.Update(promotion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return false;

            _context.Promotions.Remove(promotion);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
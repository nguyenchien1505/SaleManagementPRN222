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
    public class UserRepository : IUserRepository
    {
        private readonly WebBanHangContext _context;
        public UserRepository(WebBanHangContext context)
        {
            _context = context;
        }
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.Where(x => x.UserId == id).FirstOrDefaultAsync();
            if (user == null) return false;

            user.IsDeleted = true;
            user.DeletedDate = DateTime.Now;
            user.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.UserId == id);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        //IncludeDelete
        public async Task<List<User>> GetAllIncludeDeleteAsync()
        {
            return await _context.Users.IgnoreQueryFilters().ToListAsync();
        }

        public async Task<User> GetByEmailIncludeDeleteAsync(string email)
        {
            return await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User> GetByIdIncludeDeleteAsync(int id)
        {
            return await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.UserId == id);
        }

        public async Task<User> GetByUsernameIncludeDeleteAsync(string username)
        {
            return await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Username == username);
        }
        public async Task<bool> HasHistoricalReferencesAsync(int userId)
        {
            var customerId = await _context.Customers.Where(c => c.UserId == userId).Select(c => (int?)c.CustomerId).FirstOrDefaultAsync();

            var hasCustomerOrders = customerId.HasValue && await _context.Orders.AnyAsync(o => o.CustomerId == customerId.Value);

            var hasCreatedOrders = await _context.Orders.AnyAsync(o => o.CreatedBy == userId);

            var hasInventoryTransactions = await _context.InventoryTransactions.AnyAsync(i => i.CreatedBy == userId);

            return hasCustomerOrders || hasCreatedOrders || hasInventoryTransactions;
        }

        public async Task<bool> HardDeleteUserAsync(User user)
        {
            _context.Users.Remove(user);

            return await _context.SaveChangesAsync() > 0;
        }

        
        public async Task<User> GetByResetTokenAsync(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == token);
        }
    }
}

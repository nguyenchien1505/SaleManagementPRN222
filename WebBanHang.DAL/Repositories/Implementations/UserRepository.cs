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
            return await _context.Users.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted);
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.Where(x => x.UserId == id && !x.IsDeleted).FirstOrDefaultAsync();
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Username == username && !x.IsDeleted);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}

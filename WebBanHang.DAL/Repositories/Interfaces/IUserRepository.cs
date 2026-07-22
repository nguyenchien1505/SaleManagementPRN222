using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.DAL.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(int id);
        Task<List<User>> GetAllAsync();
        //Admin
        Task<User> GetByUsernameIncludeDeleteAsync(string username);
        Task<User> GetByEmailIncludeDeleteAsync(string email);
        Task<User> GetByIdIncludeDeleteAsync(int id);
        Task<List<User>> GetAllIncludeDeleteAsync();
        Task<bool> HasHistoricalReferencesAsync(int userId);
        Task<bool> HardDeleteUserAsync(User user);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);

        Task<User> GetByResetTokenAsync(string token);
    }
}

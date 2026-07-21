using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> GetUserProfileAsync(int userId);
        Task<bool> UpdateUserProfileAsync(int userId, string fullName, string email, string phone, string address, string? avatarUrl);
    }
}

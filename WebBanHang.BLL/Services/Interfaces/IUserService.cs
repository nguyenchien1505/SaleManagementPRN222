using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO> Login(LoginDTO dto);
        Task<bool> Register(RegisterDTO model);
        Task<IEnumerable<UserDTO>> GetAllUserAsync();
        Task<bool> CreateUserAsync(CreateUserDTO dto);
        Task<UserDTO> GetUserByIdAsync(int id);
        Task<bool> UpdateUserAsync(UpdateUserDTO dto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> RestoreUserAsync(int id);
        Task<bool> HardDeleteUserAsync(int userId, int? currentUserId);
        Task<IEnumerable<UserDTO>> GetAllUserIncludeDeleteAsync();
        Task<UserDTO> GetUserByIdIncludeDeleteAsync(int id);
        Task<UserDTO> FindOrCreateGoogleUserAsync(string email, string fullName);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}

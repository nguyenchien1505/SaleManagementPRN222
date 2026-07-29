using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Implementations;
using WebBanHang.DAL.Entities;
using static WebBanHang.BLL.Services.Implementations.UserService;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO> Login(LoginDTO dto);
        Task<bool> Register(RegisterDTO model);
        Task<IEnumerable<UserDTO>> GetAllUserAsync();
        Task<UserService.CreateUserResult> CreateUserAsync( CreateUserDTO dto,int? currentUserId);
        Task<UserDTO> GetUserByIdAsync(int id);
        Task<UpdateUserResult> UpdateUserAsync(UpdateUserDTO dto, int? currentUserId);
        Task<bool> DeleteUserAsync(int id, int? currentUserId);
        Task<bool> RestoreUserAsync(int id);
        Task<bool> HardDeleteUserAsync(int userId, int? currentUserId);
        Task<IEnumerable<UserDTO>> GetAllUserIncludeDeleteAsync();
        Task<UserDTO> GetUserByIdIncludeDeleteAsync(int id);
        Task<UserDTO> FindOrCreateGoogleUserAsync(string email, string fullName);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}

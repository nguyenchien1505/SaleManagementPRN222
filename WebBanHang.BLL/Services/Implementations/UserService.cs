using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.BLL.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        public UserService(IUserRepository userRepository)
        {
            _repo = userRepository;
        }

        public async Task<UserDTO> Login(LoginDTO dto)
        {

            var user = await _repo.GetByUsernameAsync(dto.Username);

            if (user == null || !user.IsActive || user.IsDeleted) return null;

            bool isValid = BCrypt.Net.BCrypt.Verify( dto.Password, user.PasswordHash);

            if (!isValid) return null;

            return new UserDTO
            {
                Id = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role
            };
        }

        public async Task<bool> Register(RegisterDTO dto)
        {
            var exitedUser = await _repo.GetByUsernameAsync(dto.Username);
            var exitedEmail = await _repo.GetByEmailAsync(dto.Email);

            if (exitedUser != null || exitedEmail != null) return false;

            User user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = dto.Email,
                FullName = dto.FullName,
                Role = "Customer",
                IsActive = true,
                CreatedDate = DateTime.Now,

                Customer = new Customer
                {
                    Phone = dto.Phone,
                    CreatedDate = DateTime.Now
                }
            };

            await _repo.AddAsync(user);

            return true;

        }

        public async Task<IEnumerable<UserDTO>> GetAllUserAsync()
        {
            var users = await _repo.GetAllAsync();

            return users.Select(u => new UserDTO
            {
                Id = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedDate,
                IsActived = u.IsActive

            }).ToList();

        }

        public async Task<bool> CreateUserAsync(CreateUserDTO dto)
        {
            var exitedUser = await _repo.GetByUsernameAsync(dto.Username);
            var exitedEmail = await _repo.GetByEmailAsync(dto.Email);

            if(exitedUser != null || exitedEmail != null) return false;

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = dto.Email,
                FullName = dto.FullName,
                Role = dto.Role,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.Now
            };
            await _repo.AddAsync(user);

            return true;
        }

        public async Task<bool> UpdateUserAsync(UpdateUserDTO dto)
        {
            var user = await _repo.GetByIdAsync(dto.Id);
            if(user == null) return false;

            var existedEmail = await _repo.GetByEmailAsync(dto.Email);
            if(existedEmail != null && existedEmail.UserId != dto.Id) return false;

            user.Email = dto.Email;
            user.FullName = dto.FullName;
            user.Role = dto.Role;
            user.IsActive = dto.IsActive;

            if(!dto.Password.IsNullOrEmpty())
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _repo.UpdateAsync(user);
            return true;
        }

        public async Task<UserDTO> GetUserByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return null;

            return new UserDTO
            {
                Id = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAt = user.CreatedDate,
                Role = user.Role,
                IsActived = user.IsActive
            };
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return false;

            if (user.Role == "Admin") return false;

            return await _repo.DeleteAsync(id);
        }
    }
}

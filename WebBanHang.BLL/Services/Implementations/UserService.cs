using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Implementations;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.BLL.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IEmailService _emailService;
        public UserService(IUserRepository userRepository, IEmailService emailService)
        {
            _repo = userRepository;
            _emailService = emailService;
        }

        public async Task<UserDTO> Login(LoginDTO dto)
        {

            var user = await _repo.GetByUsernameIncludeDeleteAsync(dto.Username);

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
            var exitedUser = await _repo.GetByUsernameIncludeDeleteAsync(dto.Username);
            var exitedEmail = await _repo.GetByEmailIncludeDeleteAsync(dto.Email);

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

            var existedEmail = await _repo.GetByEmailIncludeDeleteAsync(dto.Email);
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
        //Include deleted users
        public async Task<IEnumerable<UserDTO>> GetAllUserIncludeDeleteAsync()
        {
            var users = await _repo.GetAllIncludeDeleteAsync();

            return users.Select(u => new UserDTO
            {
                Id = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedDate,
                IsActived = u.IsActive,
                IsDeleted = u.IsDeleted

            }).ToList();

        }
        public async Task<UserDTO> GetUserByIdIncludeDeleteAsync(int id)
        {
            var user = await _repo.GetByIdIncludeDeleteAsync(id);
            if (user == null) return null;

            return new UserDTO
            {
                Id = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAt = user.CreatedDate,
                Role = user.Role,
                IsActived = user.IsActive,
                IsDeleted = user.IsDeleted
            };
        }

        public async Task<bool> RestoreUserAsync(int id)
        {
            var user = await _repo.GetByIdIncludeDeleteAsync(id);
            if (user == null) return false;
            user.IsDeleted = false;
            user.DeletedDate = null;
            user.IsActive = true;
            await _repo.UpdateAsync(user);
            return true;
        }

        public async Task<bool> HardDeleteUserAsync(int userId, int? currentUserId)
        {
            var user = await _repo.GetByIdIncludeDeleteAsync(userId);

            if (user == null) return false;
           
            // Chỉ hard delete sau khi đã soft delete
            if (!user.IsDeleted) 
                return false;
            
            // Không cho xóa chính tài khoản đang đăng nhập
            if (currentUserId.HasValue && currentUserId.Value == userId)           
                return false;
            

            // Không cho hard delete tài khoản admin
            if (string.Equals( user.Role, "Admin", StringComparison.OrdinalIgnoreCase))        
                return false;
            

            var hasHistory = await _repo.HasHistoricalReferencesAsync(userId);

            if (hasHistory)
                return false;

            return await _repo.HardDeleteUserAsync(user);
        }

        

        public async Task<UserDTO> FindOrCreateGoogleUserAsync(string email, string fullName)
        {
            var existing = await _repo.GetByEmailAsync(email);

            if (existing != null)
            {
                if (!existing.IsActive || existing.IsDeleted) return null;

                return new UserDTO
                {
                    Id = existing.UserId,
                    Username = existing.Username,
                    FullName = existing.FullName,
                    Role = existing.Role
                };
            }

            // Chưa có tài khoản -> tự tạo tài khoản Customer mới liên kết với email Google
            var newUser = new User
            {
                Username = email,                 // dùng email làm username cho tài khoản Google
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // mật khẩu ngẫu nhiên, không dùng tới vì đăng nhập qua Google
                Email = email,
                FullName = fullName,
                Role = "Customer",
                IsActive = true,
                CreatedDate = DateTime.Now,
                Customer = new Customer
                {
                    CreatedDate = DateTime.Now
                }
            };

            await _repo.AddAsync(newUser);

            return new UserDTO
            {
                Id = newUser.UserId,
                Username = newUser.Username,
                FullName = newUser.FullName,
                Role = newUser.Role
            };
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _repo.GetByEmailAsync(email);
            if (user == null || !user.IsActive || user.IsDeleted) return false;

            var token = Guid.NewGuid().ToString("N");
            user.ResetPasswordToken = token;
            user.ResetPasswordTokenExpiry = DateTime.Now.AddMinutes(30);

            await _repo.UpdateAsync(user);

            var resetLink = $"https://localhost:7076/Account/ResetPassword?token={token}";
            // Lưu ý: đổi domain trên đây thành domain thật khi deploy production

            var htmlBody = $@"
        <p>Xin chào {user.FullName},</p>
        <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản WebBanHang.</p>
        <p>Nhấn vào link bên dưới để đặt lại mật khẩu (link có hiệu lực trong 30 phút):</p>
        <p><a href='{resetLink}'>{resetLink}</a></p>
        <p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>";

            await _emailService.SendEmailAsync(user.Email, "Đặt lại mật khẩu WebBanHang", htmlBody);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _repo.GetByResetTokenAsync(token);

            if (user == null || user.ResetPasswordTokenExpiry == null || user.ResetPasswordTokenExpiry < DateTime.Now)
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;

            await _repo.UpdateAsync(user);
            return true;
        }
    }
}

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
        public enum UpdateUserResult
        {
            Success,
            UserNotFound,
            EmailAlreadyExists,
            CannotModifyOwnRoleOrStatus,
            CannotRemoveLastAdmin,
            VerificationRequired,
            InvalidVerificationPassword,
            CurrentUserNotFound,
            CurrentUserNotAuthorized
        }
        public enum CreateUserResult
        {
            Success,
            EmailAlreadyExists,
            VerificationRequired,
            InvalidVerificationPassword,
            CurrentUserNotFound,
            CurrentUserNotAuthorized
        }
        private readonly IUserRepository _repo;
        private readonly IEmailService _emailService;
        public UserService(IUserRepository userRepository, IEmailService emailService)
        {
            _repo = userRepository;
            _emailService = emailService;
        }

        public async Task<UserDTO> Login(LoginDTO dto)
        {

            var user = await _repo.GetByEmailIncludeDeleteAsync(dto.Email);

            if (user == null || !user.IsActive || user.IsDeleted) return null;

            bool isValid = BCrypt.Net.BCrypt.Verify( dto.Password, user.PasswordHash);

            if (!isValid) return null;

            return new UserDTO
            {
                Id = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            };
        }

        public async Task<bool> Register(RegisterDTO dto)
        {
            var exitedUser = await _repo.GetByEmailIncludeDeleteAsync(dto.Email);
            var exitedEmail = await _repo.GetByEmailIncludeDeleteAsync(dto.Email);

            if (exitedUser != null || exitedEmail != null) return false;

            User user = new User
            {
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = dto.Email,
                FullName = dto.FullName,
                Role = "Customer",
                IsActive = true,
                CreatedDate = DateTime.Now,
                Phone = dto.Phone,
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
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedDate,
                IsActived = u.IsActive

            }).ToList();

        }

        public async Task<CreateUserResult> CreateUserAsync( CreateUserDTO dto, int? currentUserId)
        {
            var existedUser = await _repo.GetByEmailIncludeDeleteAsync(dto.Email.Trim());

            if (existedUser != null)
                return CreateUserResult.EmailAlreadyExists;

            var isCreatingAdmin = string.Equals(
                dto.Role,
                "Admin",
                StringComparison.OrdinalIgnoreCase);

            if (isCreatingAdmin)
            {
                if (!currentUserId.HasValue)
                    return CreateUserResult.CurrentUserNotFound;

                if (string.IsNullOrWhiteSpace(dto.VerificationPassword))
                    return CreateUserResult.VerificationRequired;

                var currentUser = await _repo.GetByIdAsync(currentUserId.Value);

                if (currentUser == null || currentUser.IsDeleted || !currentUser.IsActive)
                {
                    return CreateUserResult.CurrentUserNotFound;
                }

                var currentUserIsAdmin = string.Equals(
                    currentUser.Role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase);

                if (!currentUserIsAdmin)
                    return CreateUserResult.CurrentUserNotAuthorized;

                var passwordIsValid = BCrypt.Net.BCrypt.Verify( dto.VerificationPassword, currentUser.PasswordHash);

                if (!passwordIsValid)
                    return CreateUserResult.InvalidVerificationPassword;
            }

            var user = new User
            {
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(dto.Password),

                Email = dto.Email.Trim(),
                FullName = dto.FullName.Trim(),
                Role = dto.Role,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.Now
            };

            await _repo.AddAsync(user);

            return CreateUserResult.Success;
        }

        public async Task<UpdateUserResult> UpdateUserAsync(UpdateUserDTO dto, int? currentUserId)
        {
            var user = await _repo.GetByIdAsync(dto.Id);

            if (user == null || user.IsDeleted)
                return UpdateUserResult.UserNotFound;

            var existedEmail = await _repo.GetByEmailIncludeDeleteAsync(dto.Email.Trim());

            if (existedEmail != null && existedEmail.UserId != dto.Id)
                return UpdateUserResult.EmailAlreadyExists;

            var isCurrentRoleAdmin = string.Equals( user.Role, "Admin",StringComparison.OrdinalIgnoreCase);

            var isNewRoleAdmin = string.Equals( dto.Role,"Admin",StringComparison.OrdinalIgnoreCase);

            var roleChanged = !string.Equals(user.Role, dto.Role, StringComparison.OrdinalIgnoreCase);

            var activeStatusChanged = user.IsActive != dto.IsActive;

            // Không cho người dùng tự đổi role hoặc tự khóa tài khoản
            if (currentUserId.HasValue && user.UserId == currentUserId.Value &&
                (roleChanged || activeStatusChanged))
            {
                return UpdateUserResult.CannotModifyOwnRoleOrStatus;
            }

            var requiresPasswordVerification = (roleChanged && (isCurrentRoleAdmin || isNewRoleAdmin))
                                         ||    (activeStatusChanged && isCurrentRoleAdmin);


            if (requiresPasswordVerification)
            {
                if (!currentUserId.HasValue)
                {
                    return UpdateUserResult.CurrentUserNotFound;
                }

                if (string.IsNullOrWhiteSpace(dto.VerificationPassword))
                {
                    return UpdateUserResult.VerificationRequired;
                }

                var currentUser = await _repo.GetByIdAsync(currentUserId.Value);

                if (currentUser == null || currentUser.IsDeleted || !currentUser.IsActive)
                {
                    return UpdateUserResult.CurrentUserNotFound;
                }

                var currentUserIsAdmin = string.Equals(
                    currentUser.Role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase);

                if (!currentUserIsAdmin)
                {
                    return UpdateUserResult.CurrentUserNotAuthorized;
                }

                var passwordIsValid = BCrypt.Net.BCrypt.Verify(
                    dto.VerificationPassword,
                    currentUser.PasswordHash);

                if (!passwordIsValid)
                {
                    return UpdateUserResult.InvalidVerificationPassword;
                }
            }


            var wasActiveAdmin = isCurrentRoleAdmin && user.IsActive;
            var willBeActiveAdmin = isNewRoleAdmin && dto.IsActive;

            if (wasActiveAdmin && !willBeActiveAdmin)
            {
                var activeAdminCount = await _repo.CountActiveAdminsAsync();

                if (activeAdminCount <= 1)
                    return UpdateUserResult.CannotRemoveLastAdmin;
            }

            user.Email = dto.Email.Trim();
            user.FullName = dto.FullName.Trim();
            user.Role = dto.Role;
            user.IsActive = dto.IsActive;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _repo.UpdateAsync(user);

            return UpdateUserResult.Success;
        }

        public async Task<UserDTO> GetUserByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return null;

            return new UserDTO
            {
                Id = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAt = user.CreatedDate,
                Role = user.Role,
                IsActived = user.IsActive
            };
        }

        public async Task<bool> DeleteUserAsync(int id, int? currentUserId)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return false;

            if (user.Role == "Admin") return false;

            if(user.UserId == currentUserId) return false;

            return await _repo.DeleteAsync(id);
        }
        //Include deleted users
        public async Task<IEnumerable<UserDTO>> GetAllUserIncludeDeleteAsync()
        {
            var users = await _repo.GetAllIncludeDeleteAsync();

            return users.Select(u => new UserDTO
            {
                Id = u.UserId,
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
            if (user == null || !user.IsDeleted) return false;
            user.IsDeleted = false;
            user.DeletedDate = null;
            user.IsActive = false;
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
                    Email = existing.Email,
                    FullName = existing.FullName,
                    Role = existing.Role
                };
            }

            // Chưa có tài khoản -> tự tạo tài khoản Customer mới liên kết với email Google
            var newUser = new User
            {
                //Username = email,                 // dùng email làm username cho tài khoản Google
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // mật khẩu ngẫu nhiên, không dùng tới vì đăng nhập qua Google
                Email = email,
                FullName = fullName,
                Role = "Customer",
                IsActive = true,
                CreatedDate = DateTime.Now,
            };

            await _repo.AddAsync(newUser);

            return new UserDTO
            {
                Id = newUser.UserId,
                Email = newUser.Email,
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

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

        public User Login(string username, string password)
        {
            var user = _repo.GetByUsername(username);
            if (user == null) return null;
            if (user.PasswordHash != password) return null;

            return user;
        }

        public bool Register(RegisterDTO dto)
        {
            var exitedUser = _repo.GetByUsername(dto.Username);

            if(exitedUser != null) return false;

            User user = new User
            {
                Username = dto.Username,
                PasswordHash = dto.Password,
                Email = dto.Email,
                FullName = dto.FullName,
                Role = "Customer",
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _repo.Add(user);

            return true;

        }
    }
}

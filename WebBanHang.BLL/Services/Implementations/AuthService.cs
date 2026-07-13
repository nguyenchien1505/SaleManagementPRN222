using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Context;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.BLL.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly WebBanHangContext _context;

        public AuthService(IUserRepository userRepository, ICustomerRepository customerRepository,WebBanHangContext context)
        {
            _userRepository = userRepository;
            _customerRepository = customerRepository;
            _context = context;
        }
        public async Task<User?> GetUserProfileAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Customer)
                    .ThenInclude(c => c.Orders)
                        .ThenInclude(o => o.OrderDetails)
                            .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, string fullName, string email, string phone, string address, string? avatarUrl)
        {
            var user = await _context.Users.FindAsync(userId);

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);

            if (user == null || customer == null) return false;

            user.FullName = fullName;
            user.Email = email;


            customer.Phone = phone;       
            customer.Address = address;  

            _context.Users.Update(user);
            _context.Customers.Update(customer);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

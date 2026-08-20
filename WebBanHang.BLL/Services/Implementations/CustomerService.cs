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
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly WebBanHangContext _context; // For complex includes

        public CustomerService(ICustomerRepository customerRepository, WebBanHangContext context)
        {
            _customerRepository = customerRepository;
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllCustomersAsync(string? searchString)
        {
            var query = _context.Users
                .Include(c => c.OrderCustomers)
                .OrderByDescending(c => c.CreatedDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(c => c.FullName.ToLower().Contains(searchString) ||
                                       c.Phone.Contains(searchString) ||
                                       c.Email.ToLower().Contains(searchString));
            }

            return await query.ToListAsync();
        }

        public async Task<User?> GetCustomerDetailsAsync(int id)
        {
            return await _context.Users
                .Include(c => c.OrderCustomers)
                    .ThenInclude(o => o.OrderDetails)
                .FirstOrDefaultAsync(m => m.UserId == id);
        }
    }
}

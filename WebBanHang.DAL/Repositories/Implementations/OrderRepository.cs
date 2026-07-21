using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Context;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.DAL.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly WebBanHangContext _context;
        public OrderRepository(WebBanHangContext context)
        {
            _context = context;
        }
        public void Add(Order order)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Order> GetAll()
        {
            throw new NotImplementedException();
        }

        public Order GetById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Order?> GetOrderDetailsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.CreatedByNavigation)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersWithCustomerAsync(string? searchString, string? statusFilter)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.CreatedByNavigation)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(o => o.OrderCode.Contains(searchString) ||
                                         o.Customer.User.FullName.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(o => o.Status == statusFilter);
            }

            return await query.ToListAsync();
        }

        public void Update(Order order)
        {
            throw new NotImplementedException();
        }
    }
}

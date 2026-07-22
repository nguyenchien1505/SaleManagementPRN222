using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Context;
using WebBanHang.DAL.Repositories.Interfaces;
using WebBanHang.DAL.QueryModels.Dashboard;

namespace WebBanHang.DAL.Repositories.Implementations
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly WebBanHangContext _context;

        public DashboardRepository(WebBanHangContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o =>
                    o.Status == "Completed" ||
                    o.Status == "Paid")
                .SumAsync(o => o.TotalAmount) ?? 0;
        }

        public async Task<int> GetTotalOrdersAsync()
        {
            return await _context.Orders.CountAsync();
        }

        public async Task<int> GetPendingOrdersAsync()
        {
            return await _context.Orders.CountAsync(o =>
                o.Status == "Pending" ||
                o.Status == "Processing");
        }

        public async Task<int> GetNewUsersAsync(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            return await _context.Users.CountAsync(u =>
                u.CreatedDate.HasValue &&
                u.CreatedDate.Value >= startDate &&
                u.CreatedDate.Value < endDate);
        }

        public async Task<List<RecentOrderResult>> GetRecentOrdersAsync(int count)
        {
            return await _context.Orders.Include(o => o.Customer)
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .Select(o => new RecentOrderResult
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount ?? 0m,
                    Status = o.Status ?? "Unknow",
                    CustomerName = o.Customer.User.FullName ?? "Unknown"
                })
                .ToListAsync();
        }

        public async Task<List<MonthlyRevenueResult>>
            GetMonthlyRevenueAsync(int year)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o =>
                    o.OrderDate.HasValue &&
                    o.OrderDate.Value.Year == year &&
                    (o.Status == "Completed" ||
                     o.Status == "Paid"))
                .GroupBy(o => o.OrderDate!.Value.Month)
                .Select(g => new MonthlyRevenueResult
                {
                    Month = g.Key,
                    TotalRevenue =
                        g.Sum(o => o.TotalAmount) ?? 0
                })
                .ToListAsync();
        }

        public async Task<List<CategoryStatisticResult>>
            GetTopCategoriesAsync(int count)
        {
            return await _context.OrderDetails
                .AsNoTracking()
                .Where(od =>
                    od.Product != null &&
                    od.Product.Category != null)
                .GroupBy(od => od.Product!.Category!.Name)
                .Select(g => new CategoryStatisticResult
                {
                    CategoryName = g.Key ?? "Unknown",
                    Quantity = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(count)
                .ToListAsync();
        }


        public async Task<int> GetTotalProductsAsync()
        {
            return await _context.Products.CountAsync(p => p.Status != "Deleted");
        }

        public async Task<int> GetTotalCustomersAsync()
        {
            return await _context.Customers.CountAsync();
        }

        public async Task<int> GetOrderCountByStatusAsync(string status)
        {
            return await _context.Orders.CountAsync(o => o.Status == status);
        }

        public async Task<List<MonthlyOrderCountResult>> GetMonthlyOrderCountAsync(int year)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Year == year)
                .GroupBy(o => o.OrderDate!.Value.Month)
                .Select(g => new MonthlyOrderCountResult
                {
                    Month = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();
        }

        public async Task<List<TopProductResult>> GetTopProductsAsync(int count)
        {
            // Logic tương tự Areas/Sale/Controllers/DashBoardController.cs
            return await _context.OrderDetails
                .AsNoTracking()
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    QuantitySold = g.Sum(od => od.Quantity),
                    Revenue = g.Sum(od => od.Total)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(count)
                .Join(_context.Products,
                      stat => stat.ProductId,
                      p => p.ProductId,
                      (stat, p) => new TopProductResult
                      {
                          ProductName = p.Name,
                          QuantitySold = stat.QuantitySold,
                          Revenue = stat.Revenue
                      })
                .ToListAsync();
        }
    }
}

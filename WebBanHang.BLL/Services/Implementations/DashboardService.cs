using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Context;

namespace WebBanHang.BLL.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly WebBanHangContext _context;

        // Các trạng thái được tính là "đã bán" (phát sinh doanh thu thật sự)
        private static readonly string[] RevenueStatuses = { "Confirmed", "Completed" };

        public DashboardService(WebBanHangContext context)
        {
            _context = context;
        }

        public async Task<DashboardOverviewDTO> GetOverviewAsync()
        {
            var overview = new DashboardOverviewDTO
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalCustomers = await _context.Customers.CountAsync(),

                TotalRevenue = await _context.Orders
                    .Where(o => RevenueStatuses.Contains(o.Status))
                    .SumAsync(o => o.TotalAmount ?? 0),

                DraftOrders = await _context.Orders.CountAsync(o => o.Status == "Draft"),
                ConfirmedOrders = await _context.Orders.CountAsync(o => o.Status == "Confirmed"),
                CompletedOrders = await _context.Orders.CountAsync(o => o.Status == "Completed"),
                CancelledOrders = await _context.Orders.CountAsync(o => o.Status == "Cancelled"),
            };

            // Top 5 sản phẩm bán chạy (chỉ tính đơn Confirmed/Completed)
            var topStats = await _context.OrderDetails
                .Where(od => RevenueStatuses.Contains(od.Order.Status))
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(od => od.Quantity),
                    Revenue = g.Sum(od => od.Total)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            var productIds = topStats.Select(x => x.ProductId).ToList();
            var products = await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            overview.TopProducts = topStats.Select(stat =>
            {
                var product = products.FirstOrDefault(p => p.ProductId == stat.ProductId);
                return new TopProductDTO
                {
                    ProductId = stat.ProductId,
                    Name = product?.Name ?? $"Sản phẩm #{stat.ProductId}",
                    ImageUrl = product?.ProductImages?.FirstOrDefault()?.ImageUrl,
                    QuantitySold = stat.TotalSold,
                    Revenue = stat.Revenue
                };
            }).ToList();

            return overview;
        }

        public async Task<List<RevenueStatDTO>> GetRevenueStatsAsync(string period)
        {
            period = (period ?? "month").ToLower();

            var orders = await _context.Orders
                .Where(o => RevenueStatuses.Contains(o.Status) && o.OrderDate != null)
                .Select(o => new { o.OrderDate, o.TotalAmount })
                .ToListAsync();

            IEnumerable<RevenueStatDTO> result;

            switch (period)
            {
                case "day":
                    // 30 ngày gần nhất
                    var fromDay = DateTime.Today.AddDays(-29);
                    result = orders
                        .Where(o => o.OrderDate!.Value.Date >= fromDay)
                        .GroupBy(o => o.OrderDate!.Value.Date)
                        .OrderBy(g => g.Key)
                        .Select(g => new RevenueStatDTO
                        {
                            Label = g.Key.ToString("dd/MM/yyyy"),
                            Revenue = g.Sum(x => x.TotalAmount ?? 0),
                            OrderCount = g.Count()
                        });
                    break;

                case "year":
                    result = orders
                        .GroupBy(o => o.OrderDate!.Value.Year)
                        .OrderBy(g => g.Key)
                        .Select(g => new RevenueStatDTO
                        {
                            Label = $"Năm {g.Key}",
                            Revenue = g.Sum(x => x.TotalAmount ?? 0),
                            OrderCount = g.Count()
                        });
                    break;

                case "month":
                default:
                    // 12 tháng gần nhất
                    var fromMonth = DateTime.Today.AddMonths(-11);
                    result = orders
                        .Where(o => o.OrderDate!.Value >= new DateTime(fromMonth.Year, fromMonth.Month, 1))
                        .GroupBy(o => new { o.OrderDate!.Value.Year, o.OrderDate!.Value.Month })
                        .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                        .Select(g => new RevenueStatDTO
                        {
                            Label = $"Thg {g.Key.Month:00}/{g.Key.Year}",
                            Revenue = g.Sum(x => x.TotalAmount ?? 0),
                            OrderCount = g.Count()
                        });
                    break;
            }

            return result.ToList();
        }

        public async Task<DashboardChartDTO> GetChartDataAsync()
        {
            var chart = new DashboardChartDTO();

            // 12 tháng gần nhất cho Revenue Chart & Order Chart
            var months = new List<(int Year, int Month)>();
            var cursor = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-11);
            for (int i = 0; i < 12; i++)
            {
                months.Add((cursor.Year, cursor.Month));
                cursor = cursor.AddMonths(1);
            }

            var fromDate = new DateTime(months.First().Year, months.First().Month, 1);
            var rawOrders = await _context.Orders
                .Where(o => o.OrderDate != null && o.OrderDate >= fromDate && RevenueStatuses.Contains(o.Status))
                .Select(o => new { o.OrderDate, o.TotalAmount })
                .ToListAsync();

            foreach (var (year, month) in months)
            {
                chart.MonthLabels.Add($"{month:00}/{year}");

                var inMonth = rawOrders.Where(o => o.OrderDate!.Value.Year == year && o.OrderDate!.Value.Month == month).ToList();
                chart.RevenueByMonth.Add(inMonth.Sum(o => o.TotalAmount ?? 0));
                chart.OrderCountByMonth.Add(inMonth.Count);
            }

            // Product Chart: top 5 sản phẩm bán chạy
            var topStats = await _context.OrderDetails
                .Where(od => RevenueStatuses.Contains(od.Order.Status))
                .GroupBy(od => od.ProductId)
                .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(od => od.Quantity) })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            var productIds = topStats.Select(x => x.ProductId).ToList();
            var productNames = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId, p => p.Name);

            foreach (var stat in topStats)
            {
                chart.TopProductNames.Add(productNames.TryGetValue(stat.ProductId, out var name) ? name : $"#{stat.ProductId}");
                chart.TopProductQuantities.Add(stat.TotalSold);
            }

            return chart;
        }
    }
}

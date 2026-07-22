using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Context;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.BLL.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IMemoryCache _cache;

        private const string DashboardCacheKey = "AdminDashboardStats";

        public DashboardService(
            IDashboardRepository dashboardRepository,
            IMemoryCache cache)
        {
            _dashboardRepository = dashboardRepository;
            _cache = cache;
        }

        public async Task<DashboardDTO> GetDashboardStatsAsync()
        {
            // 1. Kiểm tra Cache
            if (_cache.TryGetValue(DashboardCacheKey, out DashboardDTO? cachedData))
            {
                return cachedData!;
            }

            var currentDate = DateTime.Now;

            // 2. Lấy dữ liệu Doanh thu & Đơn hàng theo 12 tháng
            var monthlyRevenue = await _dashboardRepository.GetMonthlyRevenueAsync(currentDate.Year);
            var revenueData = Enumerable.Range(1, 12)
                .Select(month => monthlyRevenue.FirstOrDefault(x => x.Month == month)?.TotalRevenue ?? 0)
                .ToList();

            var monthlyOrderCounts = await _dashboardRepository.GetMonthlyOrderCountAsync(currentDate.Year);
            var orderCountData = Enumerable.Range(1, 12)
                .Select(month => monthlyOrderCounts.FirstOrDefault(x => x.Month == month)?.Count ?? 0)
                .ToList();

            // 3. Lấy dữ liệu Danh mục, Sản phẩm bán chạy & Đơn hàng gần đây
            var categories = await _dashboardRepository.GetTopCategoriesAsync(5);
            var topProducts = await _dashboardRepository.GetTopProductsAsync(5);
            var recentOrders = await _dashboardRepository.GetRecentOrdersAsync(5);

            var recentOrderDTOs = recentOrders.Select(o => new RecentOrderDTO
            {
                OrderId = o.OrderId,
                OrderDate = o.OrderDate,
                CustomerName = o.CustomerName,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
            }).ToList();

            // 4. Khởi tạo đối tượng DashboardDTO hoàn chỉnh
            var dashboard = new DashboardDTO
            {
                // Chỉ số tổng quan cơ bản
                TotalRevenue = await _dashboardRepository.GetTotalRevenueAsync(),
                TotalOrders = await _dashboardRepository.GetTotalOrdersAsync(),
                PendingOrders = await _dashboardRepository.GetPendingOrdersAsync(),
                NewUsers = await _dashboardRepository.GetNewUsersAsync(currentDate.Month, currentDate.Year),

                // Chỉ số KPI bổ sung
                TotalProducts = await _dashboardRepository.GetTotalProductsAsync(),
                TotalCustomers = await _dashboardRepository.GetTotalCustomersAsync(),
                DraftOrders = await _dashboardRepository.GetOrderCountByStatusAsync("Draft"),
                ConfirmedOrders = await _dashboardRepository.GetOrderCountByStatusAsync("Confirmed"),
                CompletedOrders = await _dashboardRepository.GetOrderCountByStatusAsync("Completed"),

                // Dữ liệu biểu đồ & danh sách
                RevenueData = revenueData,
                OrderCountData = orderCountData,

                CategoryLabels = categories.Select(x => x.CategoryName).ToList(),
                CategoryData = categories.Select(x => x.Quantity).ToList(),

                TopProducts = topProducts.Select(x => new TopProductDTO
                {
                    ProductName = x.ProductName,
                    QuantitySold = x.QuantitySold,
                    Revenue = x.Revenue
                }).ToList(),

                RecentOrders = recentOrderDTOs
            };

            // 5. Lưu vào Cache trong 5 phút
            _cache.Set(
                DashboardCacheKey,
                dashboard,
                TimeSpan.FromMinutes(5));

            return dashboard;
        }
    }
}
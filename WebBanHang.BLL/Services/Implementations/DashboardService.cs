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
            if (_cache.TryGetValue(
                DashboardCacheKey,
                out DashboardDTO? cachedData))
            {
                return cachedData!;
            }

            var currentDate = DateTime.Now;

            var monthlyRevenue =
                await _dashboardRepository.GetMonthlyRevenueAsync(
                    currentDate.Year);

            var revenueData = Enumerable.Range(1, 12)
                .Select(month =>
                    monthlyRevenue
                        .FirstOrDefault(x => x.Month == month)
                        ?.TotalRevenue ?? 0)
                .ToList();

            var categories =
                await _dashboardRepository.GetTopCategoriesAsync(5);

            var recentOrders = await _dashboardRepository.GetRecentOrdersAsync(5);
            var recentOrderDTOs = recentOrders.Select(o => new RecentOrderDTO
            {
                OrderId = o.OrderId,
                OrderDate = o.OrderDate,
                CustomerName = o.CustomerName,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
            }).ToList();

            var dashboard = new DashboardDTO
            {
                TotalRevenue =
                    await _dashboardRepository.GetTotalRevenueAsync(),

                TotalOrders =
                    await _dashboardRepository.GetTotalOrdersAsync(),

                PendingOrders =
                    await _dashboardRepository.GetPendingOrdersAsync(),

                NewUsers = await _dashboardRepository.GetNewUsersAsync(
                        currentDate.Month,
                        currentDate.Year),

                RevenueData = revenueData,

                CategoryLabels = categories
                    .Select(x => x.CategoryName)
                    .ToList(),

                CategoryData = categories
                    .Select(x => x.Quantity)
                    .ToList(),
                RecentOrders = recentOrderDTOs

            };

            _cache.Set(
                DashboardCacheKey,
                dashboard,
                TimeSpan.FromMinutes(5));

            return dashboard;
        }
    }

}

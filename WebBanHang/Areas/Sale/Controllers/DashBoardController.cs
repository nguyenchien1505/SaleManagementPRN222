using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Context;

namespace WebBanHang.Areas.Sale.Controllers
{

    [Area("Sale")]
    //[Authorize(Roles = "Sales, Admin")]
    public class DashBoardController : Controller
    {

        private readonly WebBanHangContext _context;
        private readonly IDashboardService _dashboardService;

        public DashBoardController(WebBanHangContext context, IDashboardService dashboardService)
        {
            _context = context;
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            // KPI tổng quan + thống kê đơn hàng theo trạng thái + top sản phẩm bán chạy
            var overview = await _dashboardService.GetOverviewAsync();

            // Thống kê doanh thu mặc định theo Tháng (người dùng có thể đổi qua AJAX GetRevenueStats)
            var revenueStats = await _dashboardService.GetRevenueStatsAsync("month");

            // Dữ liệu cho 3 biểu đồ: Revenue Chart / Order Chart / Product Chart
            var chartData = await _dashboardService.GetChartDataAsync();

            // Đơn hàng gần đây (giữ lại tính năng cũ)
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // Sản phẩm sắp hết hàng (giữ lại tính năng cũ)
            var lowStockCount = await _context.Products
                .CountAsync(p => p.StockQuantity < 10 && p.Status == "Active");

            ViewBag.Overview = overview;
            ViewBag.RevenueStats = revenueStats;
            ViewBag.ChartData = chartData;
            ViewBag.RecentOrders = recentOrders;
            ViewBag.LowStockCount = lowStockCount;

            return View();
        }

        // AJAX: đổi bảng thống kê doanh thu giữa Ngày / Tháng / Năm
        [HttpGet]
        public async Task<IActionResult> GetRevenueStats(string period = "month")
        {
            var stats = await _dashboardService.GetRevenueStatsAsync(period);
            return Json(stats);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Context;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[RoleAuthorize("Admin")]
    public class DashboardController : Controller
    {
        private readonly WebBanHangContext _context;
        private readonly IDashboardService _dashboardService;

        public DashboardController(WebBanHangContext context, IDashboardService dashboardService)
        {
            _context = context;
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // KPI tổng quan + thống kê đơn hàng theo trạng thái + top sản phẩm bán chạy
            var overview = await _dashboardService.GetOverviewAsync();

            // Thống kê doanh thu mặc định theo Tháng (đổi qua AJAX GetRevenueStats)
            var revenueStats = await _dashboardService.GetRevenueStatsAsync("month");

            // Dữ liệu cho 3 biểu đồ: Revenue Chart / Order Chart / Product Chart
            var chartData = await _dashboardService.GetChartDataAsync();

            // Đơn hàng gần đây
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.Overview = overview;
            ViewBag.RevenueStats = revenueStats;
            ViewBag.ChartData = chartData;
            ViewBag.RecentOrders = recentOrders;

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

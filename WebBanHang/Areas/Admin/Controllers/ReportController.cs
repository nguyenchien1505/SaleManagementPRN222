using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.DAL.Context;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[RoleAuthorize("Admin")]
    public class ReportController : Controller
    {
        private readonly WebBanHangContext _context;
        private static readonly string[] RevenueStatuses = { "Confirmed", "Completed" };
        private const int PageSize = 10;

        public ReportController(WebBanHangContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? from, DateTime? to, int pageNumber = 1)
        {
            // Mặc định: báo cáo 30 ngày gần nhất
            var toDate = (to ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);
            var fromDate = (from ?? DateTime.Today.AddDays(-29)).Date;

            var ordersInRange = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .Where(o => o.OrderDate != null && o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var vm = new ReportVM
            {
                FromDate = fromDate,
                ToDate = toDate.Date,
                TotalOrders = ordersInRange.Count,
                TotalRevenue = ordersInRange.Where(o => RevenueStatuses.Contains(o.Status)).Sum(o => o.TotalAmount ?? 0),
                DraftOrders = ordersInRange.Count(o => o.Status == "Draft"),
                ConfirmedOrders = ordersInRange.Count(o => o.Status == "Confirmed"),
                CompletedOrders = ordersInRange.Count(o => o.Status == "Completed"),
                CancelledOrders = ordersInRange.Count(o => o.Status == "Cancelled"),
            };

            // Top sản phẩm bán chạy trong khoảng thời gian đã chọn
            vm.TopProducts = ordersInRange
                .Where(o => RevenueStatuses.Contains(o.Status))
                .SelectMany(o => o.OrderDetails)
                .GroupBy(d => d.Product?.Name ?? $"#{d.ProductId}")
                .Select(g => new ReportTopProductVM
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(d => d.Quantity),
                    Revenue = g.Sum(d => d.Total)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToList();

            // Danh sách đơn hàng trong khoảng (có phân trang)
            var totalItems = ordersInRange.Count;
            var pageIndex = pageNumber < 1 ? 1 : pageNumber;
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
            if (pageIndex > totalPages) pageIndex = totalPages;

            vm.Orders = ordersInRange.Skip((pageIndex - 1) * PageSize).Take(PageSize).ToList();
            vm.PageIndex = pageIndex;
            vm.TotalPages = totalPages;

            return View(vm);
        }
    }
}

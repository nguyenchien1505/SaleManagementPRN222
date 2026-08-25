using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
using WebBanHang.Filters;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Manager.Controllers
{
    [Area("Manager")]
    [RoleAuthorize("Manager")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();

            var viewModel = new AdminDashboardVM
            {
                TotalRevenue = stats.TotalRevenue,
                TotalOrders = stats.TotalOrders,
                PendingOrders = stats.PendingOrders,
                NewUsers = stats.NewUsers,

                TotalProducts = stats.TotalProducts,
                TotalCustomers = stats.TotalCustomers,
                DraftOrders = stats.DraftOrders,
                ConfirmedOrders = stats.ConfirmedOrders,
                CompletedOrders = stats.CompletedOrders,
                OrderCountData = stats.OrderCountData,
                TopProducts = stats.TopProducts,

                RecentOrders = stats.RecentOrders?.ToList() ?? new List<RecentOrderDTO>(),
                RevenueData = stats.RevenueData ?? new List<decimal>(),
                CategoryLabels = stats.CategoryLabels ?? new List<string>(),
                CategoryData = stats.CategoryData ?? new List<int>()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetail(int id, [FromServices] IOrderService orderService)
        {
            var order = await orderService.GetOrderDetailsAsync(id);
            if (order == null)
                return NotFound(new { message = "Không tìm thấy thông tin đơn hàng." });

            var result = new
            {
                orderId = order.OrderId,
                orderCode = order.OrderCode,
                orderDate = order.OrderDate?.ToString("dd/MM/yyyy HH:mm:ss"),
                status = order.Status,
                totalAmount = order.TotalAmount ?? 0m,
                subTotal = order.SubTotal ?? 0m,
                discountAmount = order.DiscountAmount ?? 0m,
                shippingAddress = order.ShippingAddress ?? "N/A",
                shippingPhone = order.ShippingPhone ?? "N/A",
                customerName = order.Customer?.FullName ?? "Unknown",
                customerEmail = order.Customer?.Email ?? "N/A",
                customerPhone = order.Customer?.Phone ?? "N/A",
                items = order.OrderDetails.Select(od => new
                {
                    productName = od.Product?.Name ?? "Sản phẩm",
                    productCode = od.Product?.Code ?? "",
                    quantity = od.Quantity,
                    unitPrice = od.UnitPrice,
                    total = od.Total,
                    imageUrl = od.Product?.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/images/no-image.png"
                })
            };

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> Create(RegisterVM vm)
        //{
        //    if (!ModelState.IsValid) return View(vm);

        //    var dto = new RegisterVM
        //    {
        //        Email = vm.Email,
        //        FullName = vm.FullName,
        //        Email = vm.Email,
        //        Password = vm.Password,
        //        Role = vm.Role
        //    }
        //}
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebBanHang.BLL.Services.Interfaces;

namespace WebBanHang.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ──────────────────────────────────────────────
        // POST: /Customer/Order/Checkout (Mua ngay)
        // ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(int productId, int quantity = 1)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });

            var result = await _orderService.CheckoutAsync(userId.Value, productId, quantity);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(MyOrders));
            }
            else
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }
        }

        // ──────────────────────────────────────────────
        // GET: /Customer/Order/MyOrders (Lịch sử đơn)
        // ──────────────────────────────────────────────
        public async Task<IActionResult> MyOrders()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });

            var orders = await _orderService.GetMyOrdersAsync(userId.Value);
            return View(orders);
        }

        // ──────────────────────────────────────────────
        // 🌟 BỔ SUNG: POST: /Customer/Order/UpdateStatus (Hủy đơn hàng)
        // ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });

            // Bảo mật phía Server: Khách hàng chỉ được phép gửi trạng thái "Cancelled" (Hủy đơn)
            if (status != "Cancelled")
            {
                TempData["Error"] = "Thao tác không hợp lệ.";
                return RedirectToAction(nameof(MyOrders));
            }

            // Thực hiện cập nhật thông qua hàm có sẵn ở tầng Service của bạn
            var (success, message) = await _orderService.UpdateOrderStatusAsync(id, status, userId);
            if (success)
            {
                TempData["Success"] = "Hủy đơn hàng thành công!";
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction(nameof(MyOrders));
        }
    }
}
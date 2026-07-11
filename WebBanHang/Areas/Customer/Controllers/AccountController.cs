using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Context;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Customer.Controllers
{
    [Area("Customer")] 
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly WebBanHangContext _context;
        private readonly ICartService _cartService;

        public AccountController(WebBanHangContext context, IAuthService authService, ICartService cartService)
        {
            _context = context;
            _authService = authService;
            _cartService = cartService;
        }

        // ──────────────────────────────────────────────
        // GET: /Customer/Account/Profile
        // ──────────────────────────────────────────────
        // GET: /Customer/Account/Profile
        public async Task<IActionResult> Profile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var user = await _authService.GetUserProfileAsync(userId.Value);
            if (user == null) return NotFound();

            var customer = user.Customer;

            var model = new CustomerProfileVM
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = customer?.Phone,
                Address = customer?.Address,
                CreatedDate = user.CreatedDate ?? DateTime.Now, 
                Orders = customer?.Orders.OrderByDescending(o => o.OrderDate).Select(o => new OrderHistoryViewModel
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate ?? DateTime.Now,
                    TotalAmount = o.TotalAmount ?? 0,
                    Status = o.Status,
                    ProductNames = string.Join(", ", o.OrderDetails.Select(d => d.Product?.Name))
                }).ToList() ?? new List<OrderHistoryViewModel>()
            };

            return View(model);
        }

        // ──────────────────────────────────────────────
        // POST: /Customer/Account/UpdateProfile
        // ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string FullName, string Email, string Phone, string Address)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            string? avatarUrl = null;

            var success = await _authService.UpdateUserProfileAsync(userId.Value, FullName, Email, Phone, Address, avatarUrl);

            if (success)
            {
                HttpContext.Session.SetString("FullName", FullName);

                TempData["Success"] = "Cập nhật thông tin hồ sơ thành công!";
            }
            else
            {
                TempData["Error"] = "Cập nhật thất bại. Không tìm thấy thông tin tài khoản.";
            }

            return RedirectToAction(nameof(Profile));
        }


    }
}
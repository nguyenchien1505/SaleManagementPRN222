using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[RoleAuthorize("Admin")]
    // Cài đặt tài khoản của người quản trị đang đăng nhập (đổi thông tin cá nhân / mật khẩu)
    public class SettingController : Controller
    {
        private readonly IUserService _service;

        public SettingController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _service.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Account");

            var vm = new SettingVM
            {
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role,
                FullName = user.FullName,
                Email = user.Email
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(SettingVM vm)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(vm);

            var currentUser = await _service.GetUserByIdAsync(userId.Value);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var dto = new UpdateUserDTO
            {
                Id = userId.Value,
                FullName = vm.FullName,
                Email = vm.Email,
                Role = currentUser.Role,
                IsActive = currentUser.IsActived,
                Password = string.IsNullOrWhiteSpace(vm.NewPassword) ? null : vm.NewPassword
            };

            var result = await _service.UpdateUserAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("", "Cập nhật thất bại. Email có thể đã được sử dụng.");
                return View(vm);
            }

            // Cập nhật lại tên hiển thị trong session nếu có đổi
            HttpContext.Session.SetString("FullName", vm.FullName);
            HttpContext.Session.SetString("Fullname", vm.FullName);

            TempData["Success"] = "Cập nhật cài đặt tài khoản thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}

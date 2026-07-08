using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.ViewModels;

namespace WebBanHang.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _service;
        public AccountController(IUserService userService)
        {
            _service = userService;
        }
        //Login, Logout, Register
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid) return View(vm);
            
            var dto = new LoginDTO
            {
                Username = vm.Username,
                Password = vm.Password,
                RememberMe = vm.RememberMe
            };

            var user = await _service.Login(dto);

            if(user == null)
            {
                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
                return View(vm);
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);

            if (user.Role == "Admin")
            {
                return RedirectToAction( "Index", "Dashboard", new { area = "Admin" });
            }

            if (user.Role == "Sales")
            {
                return RedirectToAction( "Index", "DashBoard",new { area = "Sale" });
            }

            return RedirectToAction( "Index", "Home", new { area = "Customer" });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            TempData["Success"] = "Đăng xuất thành công";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = new RegisterDTO
            {
                Username = vm.Username,
                Email = vm.Email,
                FullName = vm.FullName, 
                Phone = vm.Phone,
                Password = vm.Password,
            };

            bool result = await _service.Register(dto);

            if (!result)
            {
                ViewBag.Error = "Tên đăng nhập hoặc email đã tồn tại.";
                return View(vm);
            }

            TempData["Success"] = "Đăng ký thành công";

            return RedirectToAction( "Login", "Account");
        }
    }
}

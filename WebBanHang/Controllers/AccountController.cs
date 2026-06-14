using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.BLL.DTOs;
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
        public IActionResult Login(LoginVM vm)
        {
            if (!ModelState.IsValid) return View(vm);
            
            var dto = new LoginDTO
            {
                Username = vm.Username,
                Password = vm.Password,
                RememberMe = vm.RememberMe
            };

            var user = _service.Login(dto.Username, dto.Password);

            if(user == null)
            {
                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
                return View(vm);
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
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
        public IActionResult Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = new RegisterDTO
            {
                Username = vm.Username,
                Email = vm.Email,
                FullName = vm.FullName,
                Phone = vm.Phone,
                Password = vm.Password,
                ConfirmPassword = vm.ConfirmPassword
            };

            bool result = _service.Register(dto);

            if (!result)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View(vm);
            }

            TempData["Success"] = "Đăng ký thành công";

            return RedirectToAction( "Login", "Account");
        }
    }
}

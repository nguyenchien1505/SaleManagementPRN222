using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;

namespace WebBanHang.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _service;
        private readonly IConfiguration _configuration;

        public AccountController(IUserService userService, IConfiguration configuration)
        {
            _service = userService;
            _configuration = configuration;
        }
        //Login, Logout, Register
        [HttpGet]
        public IActionResult Login()
        {
            System.IO.File.AppendAllText("C:\\Users\\Public\\temp_log.txt", "Login GET called at " + DateTime.Now + "\n");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = new LoginDTO
            {
                Email = vm.Email,
                Password = vm.Password,
            };

            var user = await _service.Login(dto);

            if (user == null)
            {
                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
                return View(vm);
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("FullName", user.FullName);



            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Users", new { area = "Admin" });
            }

            if (user.Role == "Manager")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Manager" });
            }

            if (user.Role == "Sales")
            {
                return RedirectToAction("Index", "DashBoard", new { area = "Sale" });
            }

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            TempData["Success"] = "Đăng xuất thành công";

            return RedirectToAction(nameof(Login), new { area = "" });
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

            return RedirectToAction("Login", "Account", new { area = "" });
        }


        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var clientId = _configuration["Authentication:Google:ClientId"];
            var clientSecret = _configuration["Authentication:Google:ClientSecret"];

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                TempData["Error"] = "Đăng nhập Google chưa được cấu hình.";
                return RedirectToAction(nameof(Login), new { area = "" });
            }

            var redirectUrl = Url.Action(nameof(GoogleResponse));
            var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal == null)
            {
                TempData["Error"] = "Đăng nhập Google thất bại.";
                return RedirectToAction(nameof(Login), new { area = "" });
            }

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var fullName = result.Principal.FindFirstValue(ClaimTypes.Name) ?? email;

            // Xoá cookie tạm của Google, vì app dùng Session, không dùng cookie auth lâu dài
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Không lấy được email từ tài khoản Google.";
                return RedirectToAction(nameof(Login), new { area = "" });
            }

            var user = await _service.FindOrCreateGoogleUserAsync(email, fullName ?? "Google User");

            if (user == null)
            {
                TempData["Error"] = "Tài khoản này đã tồn tại nhưng không thể đăng nhập bằng Google.";
                return RedirectToAction(nameof(Login), new { area = "" });
            }

            // Set session y hệt luồng Login thường (giữ nguyên logic phân quyền cũ)
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("FullName", user.FullName);

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Users", new { area = "Admin" });

            if (user.Role == "Manager")
                return RedirectToAction("Index", "Dashboard", new { area = "Manager" });

            if (user.Role == "Sales")
                return RedirectToAction("Index", "DashBoard", new { area = "Sale" });

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            await _service.ForgotPasswordAsync(vm.Email);

            // Luôn hiện thông báo giống nhau dù email có tồn tại hay không,
            // để tránh lộ thông tin email nào đã đăng ký trong hệ thống.
            TempData["Success"] = "Nếu email đã đăng ký và tồn tại trong hệ thống, chúng tôi đã gửi link đặt lại mật khẩu. Vui lòng kiểm tra hộp thư (bao gồm cả thư mục Spam / Thư rác).";
            return RedirectToAction(nameof(Login), new { area = "" });
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Link đặt lại mật khẩu không hợp lệ.";
                return RedirectToAction(nameof(Login), new { area = "" });
            }

            return View(new ResetPasswordVM { Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var success = await _service.ResetPasswordAsync(vm.Token, vm.NewPassword);

            if (!success)
            {
                ModelState.AddModelError("", "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu lại.");
                return View(vm);
            }

            TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập lại.";
            return RedirectToAction(nameof(Login), new { area = "" });
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.Filters;
using WebBanHang.ViewModels;
namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[RoleAuthorize("Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _service;
        public UsersController(IUserService service) { _service = service; }     
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _service.GetAllUserAsync();
            var vm = new UserManagementVM
            {
                Users = users,
                TotalUsers = users.Count(),
                SearchTerm = "",
                RoleFilter = "All"
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        //Tao nguoi dung moi
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = new CreateUserDTO
            {
                Username = vm.Email,
                Password = vm.Password,
                Email = vm.Email,
                FullName = vm.FullName,
                Role = vm.Role,
                IsActive = vm.IsActive
            };
            await _service.CreateUserAsync(dto);

            TempData["Success"] = "Tạo người dùng thành công!";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _service.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateUserDTO dto)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteUserAsync(id);
            if (!success)
            {
                TempData["Error"] = "Không thể xóa người dùng này.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Đã xóa người dùng thành công!";
            return RedirectToAction(nameof(Index));
        }
    }

}

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
                SearchInput = "",
                RoleFilter = "All"
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Index(UserManagementVM vm)
        {
            var users = await _service.GetAllUserAsync();
            users = vm.SortStatus == 0 ? users.OrderByDescending(x => x.CreatedAt) : users.OrderBy(x => x.CreatedAt);
            if (!vm.SearchInput.IsNullOrEmpty())
            {
                var format = vm.SearchInput.ToLower().Trim();
                users = users.Where(x => x.Username.ToLower().Contains(format) || x.FullName.ToLower().Contains(format)
                                         || x.Email.ToLower().Contains(format) || x.Role.ToLower().Contains(format));
            }

            if (!vm.RoleFilter.IsNullOrEmpty() && vm.RoleFilter != "All")
                users = users.Where(x => x.Role.Contains(vm.RoleFilter));

            vm.Users = users;
            vm.SortStatus = 1 - vm.SortStatus;
            vm.TotalUsers = users.Count();

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
            var result = await _service.CreateUserAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("", "Tài khoản email hoặc username đã tồn tại!");
                return View(vm);
            }

            TempData["Success"] = "Tạo người dùng thành công!";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _service.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            UpdateUserVM vm = new UpdateUserVM
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActived,
                Role = user.Role
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateUserVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            UpdateUserDTO dto = new UpdateUserDTO
            {
                Id = vm.UserId,
                Role = vm.Role,
                IsActive = vm.IsActive,
                FullName = vm.FullName,
                Email = vm.Email,
                Password = vm.Password.IsNullOrEmpty() ? null : vm.Password
            };

            var result = await _service.UpdateUserAsync(dto);

            if (!result)
            {
                ModelState.AddModelError("", "Tài khoản email đã tồn tại!");
                return View(vm);
            }

            TempData["Success"] = "Update successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _service.GetUserByIdAsync(id);

            if (user == null) return NotFound();
            DeleteUserVM vm = new DeleteUserVM
            {
                Id = user.Id,
                Role = user.Role,
                UserName = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
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

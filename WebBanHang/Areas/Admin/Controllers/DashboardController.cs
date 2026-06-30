using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.Filters;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[RoleAuthorize("Admin")]
    public class DashboardController : Controller
    {
        private readonly IUserService _service;

        public DashboardController(IUserService service)
        {
            _service = service;
        }

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
        //        Username = vm.Username,
        //        FullName = vm.FullName,
        //        Email = vm.Email,
        //        Password = vm.Password,
        //        Role = vm.Role
        //    }
        //}
    }
}


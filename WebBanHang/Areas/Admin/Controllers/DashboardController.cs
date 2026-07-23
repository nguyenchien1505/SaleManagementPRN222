using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
using WebBanHang.Filters;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RoleAuthorize("Admin")]
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


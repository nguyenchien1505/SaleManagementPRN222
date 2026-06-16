using Microsoft.AspNetCore.Mvc;
using WebBanHang.Filters;

namespace WebBanHang.Controllers
{
    public class DashboardController : Controller
    {
        [RoleAuthorize("Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}

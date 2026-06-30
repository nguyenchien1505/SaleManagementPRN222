using Microsoft.AspNetCore.Mvc;
using WebBanHang.Filters;

namespace WebBanHang.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        [RoleAuthorize("Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using WebBanHang.Filters;

namespace WebBanHang.Controllers
{
    public class ProductController : Controller
    {
        [RoleAuthorize("Admin", "Sales")]
        public IActionResult Index()
        {
            return View();
        }
    }
}

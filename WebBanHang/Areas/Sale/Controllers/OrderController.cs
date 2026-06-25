using Microsoft.AspNetCore.Mvc;
using WebBanHang.Filters;

namespace WebBanHang.Areas.Sale.Controllers
{
    public class OrderController : Controller
    {
        [RoleAuthorize("Customer")]
        public IActionResult Index()
        {
            return View();
        }
    }
}

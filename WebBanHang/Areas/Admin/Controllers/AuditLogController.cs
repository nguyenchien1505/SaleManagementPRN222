using Microsoft.AspNetCore.Mvc;
using WebBanHang.DAL.Context;
using WebBanHang.Filters;
using System.Linq;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RoleAuthorize("Admin")]
    public class AuditLogController : Controller
    {
        private readonly WebBanHangContext _context;

        public AuditLogController(WebBanHangContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var logs = _context.AuditLogs
                .OrderByDescending(x => x.CreatedDate)
                .Take(200)
                .ToList();

            return View(logs);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.Services.Interfaces;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[RoleAuthorize("Admin")]
    public class AuditLogController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // Xem lịch sử: ai tạo/sửa sản phẩm, ai xác nhận đơn hàng
        public async Task<IActionResult> Index()
        {
            var logs = await _auditLogService.GetRecentAsync(200);
            return View(logs);
        }
    }
}

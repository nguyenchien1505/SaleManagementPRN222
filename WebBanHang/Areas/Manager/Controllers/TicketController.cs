using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.Filters;

namespace WebBanHang.Areas.Manager.Controllers
{
    [Area("Manager")]
    [RoleAuthorize("Manager")]
    public class TicketApprovalController : Controller
    {
        private readonly ITicketService _ticketService;

        public TicketApprovalController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // GET: Manager/TicketApproval
        public async Task<IActionResult> Index()
        {
            var tickets = await _ticketService.GetPendingApprovalAsync();
            return View(tickets);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _ticketService.GetTicketDetailAsync(id);
            if (ticket == null) return NotFound();
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(ApproveTicketDTO dto)
        {
            int managerId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var success = await _ticketService.ApproveAsync(managerId, dto);
            TempData[success ? "Success" : "Error"] = success
                ? "Đã duyệt yêu cầu."
                : "Không thể duyệt (ticket không ở trạng thái chờ duyệt).";

            return RedirectToAction(nameof(Details), new { id = dto.TicketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(RejectTicketDTO dto)
        {
            int managerId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var success = await _ticketService.RejectAsync(managerId, dto);
            TempData[success ? "Success" : "Error"] = success
                ? "Đã từ chối yêu cầu."
                : "Không thể từ chối (ticket không ở trạng thái chờ duyệt).";

            return RedirectToAction(nameof(Index));
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.Filters;

namespace WebBanHang.Areas.Sale.Controllers
{
    [Area("Sale")]
    [RoleAuthorize("Sales")]
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // GET: Sale/Tickets
        public async Task<IActionResult> Index(string? statusFilter)
        {
            var tickets = await _ticketService.GetSaleQueueAsync(statusFilter);
            ViewData["CurrentStatus"] = statusFilter;
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
        public async Task<IActionResult> Start(int id)
        {
            int saleId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var success = await _ticketService.StartProcessingAsync(id, saleId);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Đã tiếp nhận xử lý."
                : "Không thể tiếp nhận (ticket không ở trạng thái Mới).";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForApproval(SubmitForApprovalDTO dto)
        {
            int saleId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var success = await _ticketService.SubmitForApprovalAsync(saleId, dto);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Đã gửi đề xuất, chờ Manager duyệt."
                : "Không thể gửi đề xuất (kiểm tra trạng thái ticket).";

            return RedirectToAction(nameof(Details), new { id = dto.TicketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            int saleId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var result = await _ticketService.CompleteTicketAsync(id, saleId);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;

namespace WebBanHang.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ICloudinaryImageService _imageService;

        public TicketController(ITicketService ticketService, ICloudinaryImageService imageService)
        {
            _ticketService = ticketService;
            _imageService = imageService;
        }

        public async Task<IActionResult> MyTickets()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });

            var tickets = await _ticketService.GetMyTicketsAsync(userId.Value);
            return View(tickets);
        }

        [HttpGet]
        public IActionResult Create(int orderDetailId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });

            ViewBag.OrderDetailId = orderDetailId;
            return View();
        }

        // POST: /Customer/Ticket/Create
        // Form phải dùng enctype="multipart/form-data", input file name="images"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketDTO dto, List<IFormFile> images)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });

            if (!ModelState.IsValid)
            {
                ViewBag.OrderDetailId = dto.OrderDetailId;
                return View(dto);
            }

            // Upload ảnh minh chứng lên Cloudinary trước khi tạo ticket
            // Controller (tầng Web) là nơi duy nhất biết IFormFile - convert sang Stream trước khi gọi BLL
            if (images != null && images.Count > 0)
            {
                var uploadedUrls = new List<string>();
                foreach (var image in images)
                {
                    if (image.Length == 0) continue;
                    await using var stream = image.OpenReadStream();
                    var url = await _imageService.UploadImageAsync(stream, image.FileName, "webbanhang/tickets");
                    if (url != null) uploadedUrls.Add(url);
                }
                dto.AttachmentUrls = uploadedUrls;
            }

            var result = await _ticketService.CreateTicketAsync(userId.Value, dto);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = result.TicketId });
            }

            TempData["Error"] = result.Message;
            ViewBag.OrderDetailId = dto.OrderDetailId;
            return View(dto);
        }

        public async Task<IActionResult> Details(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });

            var ticket = await _ticketService.GetTicketDetailAsync(id);
            if (ticket == null || ticket.CustomerId != userId.Value)
                return NotFound();

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });

            var success = await _ticketService.CancelTicketAsync(id, userId.Value);
            TempData[success ? "Success" : "Error"] = success
                ? "Đã hủy yêu cầu."
                : "Không thể hủy yêu cầu này (đã chuyển bước duyệt hoặc không thuộc về bạn).";

            return RedirectToAction(nameof(MyTickets));
        }
    }
}
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Repositories.Interfaces;
using WebBanHang.Filters;

namespace WebBanHang.Areas.Manager.Controllers
{
    [Area("Manager")]
    //[Route("Manager/[controller]")] // Định nghĩa gốc: Manager/Inventory
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProductRepository _productRepository;

        public InventoryController(IInventoryService inventoryService, IProductRepository productRepository)
        {
            _inventoryService = inventoryService;
            _productRepository = productRepository;
        }

        // ==========================================
        // 1. TRANG LỊCH SỬ KHO (Index)
        // Chấp nhận: /Admin/Inventory hoặc /Admin/Inventory/Index
        // ==========================================
        [RoleAuthorize("Manager", "Sales")]
        [HttpGet]
        [Route("Manager/Inventory")]
        [Route("Manager/Inventory/Index")]
        public IActionResult Index()
        {
            var history = _inventoryService.GetHistory();
            return View(history); // Tìm file Index.cshtml
        }

        // ==========================================
        // 2. TRANG NHẬP KHO (Inbound)
        // Chấp nhận CẢ HAI link: 
        // -> /Admin/Inventory/Inbound 
        // -> /Admin/Inventory/NhapKho
        // ==========================================
        [RoleAuthorize("Manager")]
        [HttpGet]
        [Route("Manager/Inventory/Inbound")]
        [Route("Manager/Inventory/NhapKho")]
        public IActionResult Inbound()
        {
            ViewBag.Products = _productRepository.GetAll();
            return View(); // Khớp hoàn hảo với file Inbound.cshtml của bạn
        }

        [RoleAuthorize("Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Manager/Inventory/Inbound")]
        [Route("Manager/Inventory/NhapKho")]
        public IActionResult Inbound(int productId, int quantity, string note)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (quantity <= 0)
            {
                ModelState.AddModelError("", "Số lượng nhập phải lớn hơn 0");
                ViewBag.Products = _productRepository.GetAll();
                return View();
            }

            try
            {
                _inventoryService.NhapKho(productId, quantity, userId, note);
                TempData["Success"] = "Nhập kho thành công, tồn kho đã được cập nhật.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Products = _productRepository.GetAll();
                return View();
            }
        }

        // ==========================================
        // 3. TRANG TỒN KHO HIỆN TẠI (Stock)
        // Chấp nhận CẢ HAI link: 
        // -> /Admin/Inventory/Stock 
        // -> /Admin/Inventory/TonKho
        // ==========================================
        [RoleAuthorize("Manager", "Sales")]
        [HttpGet]
        [Route("Manager/Inventory/Stock")]
        [Route("Manager/Inventory/TonKho")]
        public IActionResult Stock()
        {
            var products = _productRepository.GetAll();

            var lowStockIds = _inventoryService.GetLowStockWarnings()
                .Select(p => p.ProductId)
                .ToHashSet();

            ViewBag.LowStockIds = lowStockIds;
            return View(products); // Khớp hoàn hảo với file Stock.cshtml của bạn
        }

        [RoleAuthorize("Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Manager/Inventory/ImportInboundExcel")]
        public async Task<IActionResult> ImportInboundExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel!";
                return RedirectToAction(nameof(Inbound));
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Hệ thống chỉ hỗ trợ định dạng file .xlsx!";
                return RedirectToAction(nameof(Inbound));
            }

            try
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);

                    // Gọi hàm xử lý đọc file Excel và cộng kho ở tầng Service (đã viết ở bước trước)
                    int count = await _inventoryService.ImportInboundFromExcelAsync(stream, userId);

                    TempData["Success"] = $"Đã nhập kho thành công cho {count} mặt hàng từ file Excel!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi xử lý file Excel: " + ex.Message;
            }

            return RedirectToAction(nameof(Index)); 
        }
    }
}

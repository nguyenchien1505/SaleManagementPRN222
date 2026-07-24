using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;

namespace WebBanHang.Areas.Sale.Controllers
{
    [Area("Sale")]
    //[Authorize(Roles = "Sales, Admin")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;

        public OrdersController(IOrderService orderService, ICustomerService customerService, IProductService productService)
        {
            _orderService = orderService;
            _customerService = customerService;
            _productService = productService;
        }

        // GET: Sale/Orders
        public async Task<IActionResult> Index(string searchString, string statusFilter, int? pageNumber)
        {
            var orders = await _orderService.GetOrdersOverviewAsync(searchString, statusFilter);
            int pageSize = 10;
            var pagedOrders = Models.PaginatedList<Order>.Create(orders.AsQueryable(), pageNumber ?? 1, pageSize);

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;
            return View(pagedOrders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _orderService.GetOrderDetailsAsync(id.Value);
            if (order == null) return NotFound();

            return View(order);
        }

  

  

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus) 
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, newStatus);

            if (result)
            {
                TempData["SuccessMessage"] = "Cập nhật trạng thái thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật trạng thái.";
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}

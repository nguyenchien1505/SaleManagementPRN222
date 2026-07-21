using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Sale.Controllers
{   
    [Area("Sale")]
    public class ProductController : Controller
    {
        

        private readonly IProductService _service;
        private readonly ICategoryService _cateService;
        private readonly IWebHostEnvironment _env;
        public ProductController(IProductService service, ICategoryService ceteService, IWebHostEnvironment env)
        {
            _service = service;
            _cateService = ceteService;
            _env = env;
        }
        private const int PageSize = 10;

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var products = await _service.GetAllProductsAsync();

            var totalItems = products.Count();
            var pageIndex = page < 1 ? 1 : page;
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
            if (pageIndex > totalPages) pageIndex = totalPages;

            var vm = new ProductManagementVM
            {
                Products = products.Skip((pageIndex - 1) * PageSize).Take(PageSize),
                PageIndex = pageIndex,
                PageSize = PageSize,
                TotalPages = totalPages
            };

            // ViewBag dùng bởi thanh phân trang có sẵn trong Views/Product/Index.cshtml
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageIndex;

            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _service.GetDetailProductByIdAsync(id);
            if (product == null) return NotFound();

            var vm = new DetailProductVM
            {
                ProductId = product.ProductId,
                Code = product.Code,
                Name = product.Name,
                CategoryName = product.CategoryName,
                SellingPrice = product.SellingPrice,
                ImportPrice = product.ImportPrice,
                StockQuantity = product.StockQuantity,
                Status = product.Status,
                Description = product.Description,
                ExistingImages = product.ExistingImages.Select(x => new ProductImageDTO
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                }).ToList(),
                CreateBy = product.CreateBy,
                CreatedDate = product.CreatedDate
            };

            return View(vm);

        }
    }
}

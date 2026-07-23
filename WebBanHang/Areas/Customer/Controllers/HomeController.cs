using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.Models;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {

        private readonly IProductService _prodService;
        private readonly ICategoryService _cateService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ICategoryService cateService, IProductService prodService)
        {
            _logger = logger;
            _cateService = cateService;
            _prodService = prodService;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string? SearchString, string? sortOrder, int? categoryId, int pageNumber = 1)
        {
            int pageSize = 8; 

            var categories = await _cateService.GetAllCateAsync();
            var products = await _prodService.GetAllProductsAsync();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                products = products.Where(p => p.Name.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
                                            || p.Code.Contains(SearchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            products = sortOrder switch
            {
                "name_desc" => products.OrderByDescending(p => p.Name).ToList(),
                "Price" => products.OrderBy(p => p.SellingPrice).ToList(),
                "price_desc" => products.OrderByDescending(p => p.SellingPrice).ToList(),
                "Date" => products.OrderBy(p => p.CreatedDate).ToList(),
                _ => products.OrderByDescending(p => p.CreatedDate).ToList(),
            };

            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageNumber = pageNumber > totalPages && totalPages > 0 ? totalPages : pageNumber;

            var pagedProducts = products.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var vm = new HomeProductVM
            {
                Products = pagedProducts,
                Categories = categories,
                CategoryId = categoryId,
                SearchString = SearchString,
                SortOrder = sortOrder,
                CurrentPage = pageNumber,
                TotalPages = totalPages == 0 ? 1 : totalPages
            };

            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
    
            var product = await _prodService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _cateService.GetAllCateAsync();
            var vm = new ProductDetailVM
            {
                ProductId = id,
                Code = product.Code,
                Name = product.Name,
                Description = product.Description,
                SellingPrice = product.SellingPrice,
                Images = product.Images.ToList(),
                StockQuantity = product.StockQuantity,
                Status = product.Status,
                CreatedDate = product.CreatedDate,
                Categories = categories.ToList()
            };
            Console.WriteLine(vm.ProductId);
            return View(vm);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

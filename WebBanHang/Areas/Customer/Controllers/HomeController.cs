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
        public async Task<IActionResult> Index()
        {
            var categories = await _cateService.GetAllCateAsync();
            var products = await _prodService.GetAllProductsAsync();

            var vm = new HomeProductVM
            {
                Products = products,
                Categories = categories
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

using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProductController : Controller
    {
        private readonly IProductService _prodService;
        private readonly ICategoryService _cateService;

        public ProductController(ICategoryService cateService, IProductService prodService)
        {
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
    }
}

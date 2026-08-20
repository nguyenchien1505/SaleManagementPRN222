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
        public async Task<IActionResult> Index(int? categoryId, string? sortOrder, string? SearchString, int pageNumber = 1)
        {
            int pageSize = 9;

            var categories = await _cateService.GetAllCateAsync();
            var allProducts = await _prodService.GetAllProductsAsync();

            if (categoryId.HasValue)
            {
                allProducts = allProducts.Where(p => p.CategoryId == categoryId.Value).ToList();
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                allProducts = allProducts.Where(p => p.Name.Contains(SearchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            switch (sortOrder)
            {
                case "name_desc":
                    allProducts = allProducts.OrderByDescending(p => p.Name).ToList();
                    break;
                case "Price":
                    allProducts = allProducts.OrderBy(p => p.SellingPrice).ToList();
                    break;
                case "price_desc":
                    allProducts = allProducts.OrderByDescending(p => p.SellingPrice).ToList();
                    break;
                default:
                    allProducts = allProducts.OrderByDescending(p => p.ProductId).ToList();
                    break;
            }

            int totalItems = allProducts.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageNumber = pageNumber > totalPages && totalPages > 0 ? totalPages : pageNumber;

            var pagedProducts = allProducts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var vm = new HomeProductVM
            {
                Products = pagedProducts,
                Categories = categories,
                CategoryId = categoryId,
                SortOrder = sortOrder,
                SearchString = SearchString,
                CurrentPage = pageNumber,
                TotalPages = totalPages == 0 ? 1 : totalPages
            };

            return View(vm);
        }
    }
}
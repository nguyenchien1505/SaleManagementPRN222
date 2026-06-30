using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.Filters;
using WebBanHang.ViewModels;


namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[RoleAuthorize("Admin")]
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
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _service.GetAllProductsAsync();
            var vm = new ProductManagementVM
            {
                Products = products
            };
            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var cates = await _cateService.GetAllCateAsync();

            var vm = new CreateProductVM
            {
                Categories = cates.Select(x => new CategoryVM
                {
                    Id = x.CategoryId,
                    Name = x.Name
                }).ToList()
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM vm)
        {

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!ModelState.IsValid)
            {
                var cates = await _cateService.GetAllCateAsync();
                vm.Categories = cates.Select(x => new CategoryVM
                {
                    Id = x.CategoryId,
                    Name = x.Name,
                }).ToList();
                return View(vm);
            }

            var imageDtos = new List<ProductImageDTO>();

            if (vm.ImageFiles != null)
            {
                foreach (var file in vm.ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var folder = Path.Combine(_env.WebRootPath, "images");
                        if (!Directory.Exists(folder))   Directory.CreateDirectory(folder);
                        var filePath = Path.Combine(folder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        imageDtos.Add(new ProductImageDTO
                        {
                            ImageUrl = "/images/" + fileName,
                            IsPrimary = imageDtos.Count == 0
                        });
                    }
                }
            }

            var dto = new ProductDTO
            {
                Code = vm.Code,
                Name = vm.Name,
                CategoryId = vm.CategoryId,
                Images = imageDtos,
                Description = vm.Description,
                ImportPrice = vm.ImportPrice,
                SellingPrice = vm.SellingPrice,
                Status = vm.Status,
                StockQuantity = vm.StockQuantity,
                CreatedBy = userId.Value
            };
            var result = await _service.CreateProductAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("", "Mã sản phẩm đã tồn tại!");
                return View(vm);
            }

            TempData["Success"] = "Tạo sản phẩm thành công!";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _service.GetProductByIdAsync(id);
            var cate = await _cateService.GetAllCateAsync();
            if (product == null) return NotFound();
            var vm = new UpdateProductVM
            {
                Code = product.Code,
                Name = product.Name,
                CategoryId = product.CategoryId,
                Categories = cate.Select(x => new CategoryDTO
                {
                    CategoryId = x.CategoryId,
                    Name = x.Name
                }).ToList(),
                ExistingImages = product.Images.Select(x => new ProductImageDTO
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                }).ToList(),
                Description = product.Description,
                SellingPrice    = product.SellingPrice,
                ImportPrice = product.ImportPrice,
                StockQuantity = product.StockQuantity,
                Status = product.Status,
                CreatedBy = product.CreatedBy,
                CreatedDate = product.CreatedDate
            };
            return View(vm);
        }

        //[HttpPost]
        //public async Task<IActionResult> Update(int id)
        //{
        //    var result
        //    return
        //}

    }
}



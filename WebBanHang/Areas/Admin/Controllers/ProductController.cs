using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
using WebBanHang.Filters;
using WebBanHang.ViewModels;
using static System.Net.Mime.MediaTypeNames;


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

        private async Task LoadEditData(UpdateProductVM vm)
        {
            var cate = await _cateService.GetAllCateAsync();

            vm.Categories = cate.Select(x => new CategoryDTO
            {
                CategoryId = x.CategoryId,
                Name = x.Name
            }).ToList();

            if (vm.Id > 0)
            {
                var product = await _service.GetProductByIdAsync(vm.Id);

                vm.ExistingImages = product?.Images?.Select(x => new ProductImageDTO
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                }).ToList() ?? new List<ProductImageDTO>();
            }
            else
            {
                vm.ExistingImages ??= new List<ProductImageDTO>();
            }
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
                        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
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
                CategoryName = vm.Categories.Select(c => c.Name).ToString(),
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
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _service.GetProductByIdAsync(id);
            var cate = await _cateService.GetAllCateAsync();

            if (product == null) return NotFound();
            var vm = new UpdateProductVM
            {
                Id = product.ProductId,
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
                SellingPrice = product.SellingPrice,
                ImportPrice = product.ImportPrice,
                StockQuantity = product.StockQuantity,
                Status = product.Status,
                CreatedBy = product.CreatedBy,
                CreatedDate = product.CreatedDate
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateProductVM vm)
        {
            if (vm == null) return NotFound();

            Console.WriteLine("VM Id = " + vm.Id);
            Console.WriteLine("VM Code = " + vm.Code);

            if (!ModelState.IsValid)
            {
                await LoadEditData(vm);
                return View(vm);
            }

            var imageDtos = vm.ExistingImages ?? new List<ProductImageDTO>();

            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(vm.ImageFile.FileName);
                var folder = Path.Combine(_env.WebRootPath, "images");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(stream);
                }

                imageDtos.Add(new ProductImageDTO
                {
                    ImageUrl = "/images/" + fileName,
                    IsPrimary = true
                });
            }

            var dto = new ProductDTO
            {
                ProductId = vm.Id,
                Code = vm.Code,
                Name = vm.Name,
                CategoryId = vm.CategoryId,
                SellingPrice = vm.SellingPrice,
                Status = vm.Status,
                CategoryName = vm.CategoryName,
                CreatedBy = vm.CreatedBy,
                CreatedDate = vm.CreatedDate,
                Description = vm.Description,
                ImportPrice = vm.ImportPrice,
                StockQuantity = vm.StockQuantity,
                Images = imageDtos
            };

            try
            {
                var result = await _service.UpdateProductAsync(dto);

                if (!result)
                {
                    ModelState.AddModelError("", "Cập nhật sản phẩm thất bại. Vui lòng kiểm tra lại thông tin.");
                    await LoadEditData(vm);
                    return View(vm);
                }
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;

                if (message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("trùng", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("Code", "Mã sản phẩm đã tồn tại.");
                }
                else if (message.Contains("Không tìm thấy sản phẩm", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("", "Không tìm thấy sản phẩm cần cập nhật.");
                }
                else
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật sản phẩm: " + message);
                }

                await LoadEditData(vm);
                return View(vm);
            }

            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Index");
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

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _service.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            var vm = new DeleteProductVM
            {
                ProductId = id,
                CategoryName = product.CategoryName,
                Code = product.Code,
                Name = product.Name,
                SellingPrice = product.SellingPrice,
                StockQuantity = product.StockQuantity,
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(DeleteProductVM vm)
        {
            Console.WriteLine("_________________________________________");
            Console.WriteLine(vm.ProductId);
            if (vm == null || vm.ProductId <= 0)
            {
                TempData["Error"] = "Sản phẩm không hợp lệ.";
                return RedirectToAction("Index");
            }
            var result = await _service.DeleteProductAsync(vm.ProductId);

            if (!result)
            {
                TempData["Error"] = "Xóa sản phẩm thất bại. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }
            TempData["Success"] = "Xóa sản phẩm thành công!";
            return RedirectToAction("Index");
        }

    }
}



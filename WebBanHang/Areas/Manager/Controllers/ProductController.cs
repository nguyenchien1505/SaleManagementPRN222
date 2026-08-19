using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.InteropServices;
using System.Security.Claims;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
using WebBanHang.Filters;
using WebBanHang.ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace WebBanHang.Areas.Manager.Controllers
{
    [Area("Manager")]
    [RoleAuthorize("Manager")]
    public class ProductController : Controller
    {
        private readonly IProductService _service;
        private readonly ICategoryService _cateService;
        private readonly IWebHostEnvironment _env;
        private readonly IAuditLogService _auditLogService; // Inject thêm AuditLogService

        public ProductController(
            IProductService service,
            ICategoryService ceteService,
            IWebHostEnvironment env,
            IAuditLogService auditLogService)
        {
            _service = service;
            _cateService = ceteService;
            _env = env;
            _auditLogService = auditLogService;
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
                var product = await _service.GetProductByIdIncludeDeleteAsync(vm.Id);

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
            var products = await _service.GetAllProductsIncludeDeleteAsync();
            var categories = await _cateService.GetAllCateAsync();
            var vm = new ProductManagementVM
            {
                Products = products,
                Categories = categories.Select(c => new CategoryVM
                {
                    Id = c.CategoryId,
                    Name = c.Name
                }).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProductManagementVM vm)
        {
            var products = await _service.GetAllProductsIncludeDeleteAsync();
            var categories = await _cateService.GetAllCateAsync();

            if (!vm.SearchInput.IsNullOrEmpty())
            {
                var format = vm.SearchInput.ToLower().Trim();
                products = products.Where(p => p.Code.ToLower().Contains(format) || p.Name.ToLower().Contains(format) || p.CategoryName.ToLower().Contains(format)).ToList();
            }

            if (!vm.CateFilter.IsNullOrEmpty() && !vm.CateFilter.Contains("All"))
                products = products.Where(x => x.CategoryId.ToString() == vm.CateFilter).ToList();

            if (!vm.StatusFilter.IsNullOrEmpty() && !vm.StatusFilter.Contains("All"))
                products = products.Where(p => p.Status == vm.StatusFilter).ToList();

            switch (vm.SortColumn)
            {
                case "Code": products = vm.SortStatus == 0 ? products.OrderBy(p => p.Code).ToList() : products.OrderByDescending(p => p.Code).ToList(); break;
                case "Name": products = vm.SortStatus == 0 ? products.OrderBy(p => p.Name).ToList() : products.OrderByDescending(p => p.Name).ToList(); break;
                case "Price": products = vm.SortStatus == 0 ? products.OrderBy(p => p.SellingPrice).ToList() : products.OrderByDescending(p => p.SellingPrice).ToList(); break;
                case "Stock": products = vm.SortStatus == 0 ? products.OrderBy(p => p.StockQuantity).ToList() : products.OrderByDescending(p => p.StockQuantity).ToList(); break;
            }

            vm.Products = products;
            vm.Categories = categories.Select(c => new CategoryVM
            {
                Id = c.CategoryId,
                Name = c.Name
            }).ToList();

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
                CategoryName = vm.Categories?.FirstOrDefault(c => c.Id == vm.CategoryId)?.Name ?? string.Empty,
                Images = imageDtos,
                Description = vm.Description,
                ImportPrice = vm.ImportPrice,
                SellingPrice = vm.SellingPrice,
                Status = vm.Status,
                StockQuantity = vm.StockQuantity,
                CreatedBy = userId ?? 0
            };

            var result = await _service.CreateProductAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("", "Mã sản phẩm đã tồn tại!");
                return View(vm);
            }

            TempData["Success"] = "Tạo sản phẩm thành công!";

            // Ghi nhận Log tạo sản phẩm
            //await _auditLogService.LogAsync(
            //"Product", dto.ProductId, "CreateProduct",
            //userId ?? 0, HttpContext.Session.GetString("FullName"),
            //$"Tạo sản phẩm {dto.Code} - {dto.Name}");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _service.GetProductByIdIncludeDeleteAsync(id);
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

                ExistingImages = product.Images?.Select(x => new ProductImageDTO
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                }).ToList() ?? new List<ProductImageDTO>(),
                Description = product.Description,
                SellingPrice = product.SellingPrice,
                ImportPrice = product.ImportPrice,
                StockQuantity = product.StockQuantity,
                Status = product.Status,
                CreatedBy = product.CreatedBy,
                CreatedDate = product.CreatedDate ?? DateTime.Now
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateProductVM vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadEditData(vm);
                return View(vm);
            }

            var products = await _service.GetProductByIdIncludeDeleteAsync(vm.Id);
            if (products == null)
            {
                ModelState.AddModelError("", "Không tìm thấy sản phẩm cần cập nhật.");
                await LoadEditData(vm);
                return View(vm);
            }

            var imageDtos = products.Images?.Select(x => new ProductImageDTO
            {
                ImageUrl = x.ImageUrl,
                IsPrimary = x.IsPrimary
            }).ToList() ?? new List<ProductImageDTO>();

            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var extension = Path.GetExtension(vm.ImageFile.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.ImageFile), "Chỉ cho phép file JPG, JPEG, PNG hoặc WEBP.");
                    await LoadEditData(vm);
                    return View(vm);
                }

                var fileName = $"{Guid.NewGuid():N}{extension}";
                var folder = Path.Combine(_env.WebRootPath, "images");

                Directory.CreateDirectory(folder);

                var filePath = Path.Combine(folder, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await vm.ImageFile.CopyToAsync(stream);
                }

                foreach (var image in imageDtos)
                {
                    image.IsPrimary = false;
                }

                imageDtos.Add(new ProductImageDTO
                {
                    ImageUrl = $"/images/{fileName}",
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

            // Bước 5b: Ghi nhận Log sửa sản phẩm
            //await _auditLogService.LogAsync(
            //"Product", vm.Id, "EditProduct",
            //HttpContext.Session.GetInt32("UserId") ?? 0, HttpContext.Session.GetString("FullName"),
            //$"Cập nhật sản phẩm {vm.Code}");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _service.GetDetailProductByIdIncludeDeleteAsync(id);
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
            var product = await _service.GetProductByIdIncludeDeleteAsync(id);
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

        [HttpGet]
        public async Task<IActionResult> Restore(int id)
        {
            var product = await _service.GetProductByIdIncludeDeleteAsync(id);

            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            if (product.Status != "Deleted")
            {
                TempData["Error"] = "Sản phẩm này không ở trạng thái đã xóa.";
                return RedirectToAction(nameof(Index));
            }

            var model = new RestoreProductVM
            {
                ProductId = product.ProductId,
                Code = product.Code,
                Name = product.Name,
                CategoryName = product.CategoryName,
                SellingPrice = product.SellingPrice,
                StockQuantity = product.StockQuantity
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(DeleteProductVM vm)
        {
            var product = await _service.GetProductByIdIncludeDeleteAsync(vm.ProductId);

            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm cần khôi phục.";
                return RedirectToAction(nameof(Index));
            }

            if (product.Status != "Deleted")
            {
                TempData["Error"] = "Sản phẩm này đã được khôi phục hoặc chưa bị xóa.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _service.RestoreProductAsync(vm.ProductId);
            if (!result)
            {
                TempData["Error"] = $"Khôi phục sản phẩm \"{product.Name}\" thất bại. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }

            TempData["Success"] = $"Đã khôi phục sản phẩm \"{product.Name}\" thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> HardDelete(int id)
        {
            var product = await _service.GetProductByIdIncludeDeleteAsync(id);

            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            if (product.Status != "Deleted")
            {
                TempData["Error"] = "Phải xóa mềm sản phẩm trước khi xóa vĩnh viễn.";

                return RedirectToAction(nameof(Index));
            }

            var vm = new DeleteProductVM
            {
                ProductId = product.ProductId,
                Code = product.Code,
                Name = product.Name,
                CategoryName = product.CategoryName,
                SellingPrice = product.SellingPrice,
                StockQuantity = product.StockQuantity
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(DeleteProductVM vm)
        {
            if (vm.ProductId <= 0)
            {
                TempData["Error"] = "Sản phẩm không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var product = await _service.GetProductByIdIncludeDeleteAsync(vm.ProductId);

            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            if (product.Status != "Deleted")
            {
                TempData["Error"] = "Chỉ được xóa vĩnh viễn sản phẩm đã xóa mềm.";

                return RedirectToAction(nameof(Index));
            }

            var imageUrls = product.Images?.Select(x => x.ImageUrl).Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();

            var result = await _service.HardDeleteProductAsync(vm.ProductId);

            if (!result)
            {
                TempData["Error"] = "Không thể xóa vĩnh viễn. Sản phẩm có thể đang được sử dụng trong đơn hàng hoặc lịch sử kho.";

                return RedirectToAction(nameof(Index));
            }

            DeleteProductImageFiles(imageUrls);

            TempData["Success"] = $"Đã xóa vĩnh viễn sản phẩm \"{product.Name}\".";

            return RedirectToAction(nameof(Index));
        }

        private void DeleteProductImageFiles(IEnumerable<string> imageUrls)
        {
            foreach (var imageUrl in imageUrls)
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    continue;
                }

                var fileName = Path.GetFileName(imageUrl);

                if (string.IsNullOrWhiteSpace(fileName) ||
                    fileName.Equals("no-image.png", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

                try
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        // ──────────────────────────────────────────────
        // CHỨC NĂNG EXCEL
        // ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                var fileContent = await _service.ExportProductsToExcelAsync();

                string fileName = $"Danh_Sach_SanPham_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xuất file Excel: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel!";
                return RedirectToAction(nameof(Index));
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Hệ thống chỉ hỗ trợ định dạng file .xlsx!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);

                    int importedCount = await _service.ImportProductsFromExcelAsync(stream, userId);

                    TempData["Success"] = $"Đã nhập thành công {importedCount} sản phẩm từ file Excel!";

                    await _auditLogService.LogAsync(
                        "Product", 0, "ImportExcel",
                        userId, HttpContext.Session.GetString("FullName"),
                        $"Đã Import {importedCount} sản phẩm bằng file Excel"
                    );
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Implementations;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.Filters;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RoleAuthorize("Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _cateService;
        public CategoryController(ICategoryService cateService)
        {
            _cateService = cateService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _cateService.GetAllCateIncludeDeleteAsync();

            if (categories == null) return NotFound();

            var vm = new CategoryManagementVM
            {
                Categories = categories,
                SearchInput = "",
                SortStatus = 0,
                StatusFilter = "All"
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Index(CategoryManagementVM vm)
        {
            var categories = await _cateService.GetAllCateIncludeDeleteAsync();
            categories = vm.SortStatus == 0 ? categories.OrderByDescending(x => x.Name).ToList() : categories.OrderBy(x => x.Name).ToList();

            if (!vm.SearchInput.IsNullOrEmpty())
            {
                var format = vm.SearchInput.ToLower().Trim();
                categories = categories.Where(x => x.Name.ToLower().Contains(format) || (x.Description ?? "").ToLower().Contains(format)).ToList();
            }
            if (!vm.StatusFilter.IsNullOrEmpty() && vm.StatusFilter != "All")
                categories = categories.Where(x => (x.Status ?? "").Contains(vm.StatusFilter)).ToList();

            vm.Categories = categories;
            vm.SortStatus = 1 - vm.SortStatus;

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var dto = new CreateCategoryDTO
            {
                Name = vm.Name,
                Description = vm.Description,
                Status = vm.Status
            };
            var result = await _cateService.AddCateAsync(dto);
            if(!result)
            {
                ModelState.AddModelError("", "Thêm danh mục thất bại. Vui lòng thử lại");
                return View(vm);
            }
            TempData["Success"] = "Thêm danh mục thành công";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cate = await _cateService.GetCateByIdAsync(id);
            if (cate == null) return NotFound();
            var vm = new UpdateCategoryVM
            {
                CategoryId = cate.CategoryId,
                Name = cate.Name,
                Description = cate.Description,
                Status = cate.Status
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCategoryVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var dto = new UpdateCategoryDTO
            {
                CategoryId = vm.CategoryId,
                Name = vm.Name,
                Description = vm.Description,
                Status = vm.Status
            };
            var result = await _cateService.UpdateCateAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("", "Cập nhật danh mục thất bại. Vui lòng thử lại");
                return View(vm);
            }
            TempData["Success"] = "Cập nhật danh mục thành công";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var cate = await _cateService.GetCateByIdAsync(id);
            if (cate == null) return NotFound();
            var vm = new DeleteCategoryVM
            {
                CategoryId = cate.CategoryId,
                Name = cate.Name,
                Description = cate.Description,
                Status = cate.Status
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(DeleteCategoryVM vm)
        {
            var result = await _cateService.DeleteCateAsync(vm.CategoryId);
            if (!result)
            {
                ModelState.AddModelError("", "Xóa danh mục thất bại. Vui lòng thử lại");
                return View(vm);
            }
            TempData["Success"] = "Xóa danh mục thành công";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _cateService.RestoreCateAsync(id);

            if (result)
            {
                TempData["Success"] = "Khôi phục danh mục thành công!";
            }
            else
            {
                TempData["Error"] = "Khôi phục danh mục thất bại. Vui lòng thử lại!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            var result =
                await _cateService.HardDeleteCateAsync(id);

            if (result)
            {
                TempData["Success"] = "Danh mục đã được xóa vĩnh viễn thành công!";
            }
            else
            {
                TempData["Error"] = "Hành động xóa cứng danh mục thất bại. Vui lòng thử lại!";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}

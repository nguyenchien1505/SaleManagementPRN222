using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.Filters;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[RoleAuthorize("Admin")]
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
            var categories = await _cateService.GetAllCateAsync();

            if (categories == null) return NotFound();

            var vm = new CategoryManagementVM
            {
                Categories = categories
            };

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

    }
}

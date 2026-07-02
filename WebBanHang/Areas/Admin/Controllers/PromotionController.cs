using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PromotionController : Controller
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        public async Task<IActionResult> Index(string? statusFilter, string? searchString)
        {
            var dtos = await _promotionService.GetAllPromotionsAsync(statusFilter, searchString);
            var vms = dtos.Select(d => new PromotionVM
            {
                PromotionId = d.PromotionId,
                Code = d.Code,
                DiscountType = d.DiscountType,
                Value = d.Value,
                MinOrderValue = d.MinOrderValue,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                Status = d.Status
            }).ToList();

            ViewData["CurrentFilter"] = statusFilter;
            ViewData["CurrentSearch"] = searchString;
            return View(vms);
        }

        public async Task<IActionResult> Details(int id)
        {
            var d = await _promotionService.GetPromotionByIdAsync(id);
            if (d == null) return NotFound();

            var vm = new PromotionVM
            {
                PromotionId = d.PromotionId,
                Code = d.Code,
                DiscountType = d.DiscountType,
                Value = d.Value,
                MinOrderValue = d.MinOrderValue,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                Status = d.Status
            };
            return View(vm);
        }

        public IActionResult Create()
        {
            return View(new PromotionVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionVM vm)
        {
            if (ModelState.IsValid)
            {
                var dto = new PromotionDTO
                {
                    Code = vm.Code,
                    DiscountType = vm.DiscountType,
                    Value = vm.Value,
                    MinOrderValue = vm.MinOrderValue,
                    StartDate = vm.StartDate,
                    EndDate = vm.EndDate,
                    Status = vm.Status
                };

                if (await _promotionService.AddPromotionAsync(dto))
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");
            }
            return View(vm);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var d = await _promotionService.GetPromotionByIdAsync(id);
            if (d == null) return NotFound();

            var vm = new PromotionVM
            {
                PromotionId = d.PromotionId,
                Code = d.Code,
                DiscountType = d.DiscountType,
                Value = d.Value,
                MinOrderValue = d.MinOrderValue,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                Status = d.Status
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PromotionVM vm)
        {
            if (id != vm.PromotionId) return NotFound();

            if (ModelState.IsValid)
            {
                var dto = new PromotionDTO
                {
                    PromotionId = vm.PromotionId,
                    Code = vm.Code,
                    DiscountType = vm.DiscountType,
                    Value = vm.Value,
                    MinOrderValue = vm.MinOrderValue,
                    StartDate = vm.StartDate,
                    EndDate = vm.EndDate,
                    Status = vm.Status
                };

                if (await _promotionService.UpdatePromotionAsync(dto))
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Không thể cập nhật thông tin khuyến mãi.");
            }
            return View(vm);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var d = await _promotionService.GetPromotionByIdAsync(id);
            if (d == null) return NotFound();

            var vm = new PromotionVM
            {
                PromotionId = d.PromotionId,
                Code = d.Code,
                DiscountType = d.DiscountType,
                Value = d.Value,
                MinOrderValue = d.MinOrderValue,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                Status = d.Status
            };
            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _promotionService.DeletePromotionAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
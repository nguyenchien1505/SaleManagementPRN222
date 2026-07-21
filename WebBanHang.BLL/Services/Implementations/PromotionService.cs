using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Implementations;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.BLL.Services.Implementations
{
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepo;

        public PromotionService(IPromotionRepository promotionRepo)
        {
            _promotionRepo = promotionRepo;
        }

        public async Task<IEnumerable<PromotionDTO>> GetAllPromotionsAsync(string? statusFilter, string? searchString)
        {
            var promotions = await _promotionRepo.GetAllAsync(null, null);
            return promotions.Select(p => new PromotionDTO
            {
                PromotionId = p.PromotionId,
                Code = p.Code,
                DiscountType = p.DiscountType,
                Value = p.Value,
                MinOrderValue = p.MinOrderValue ?? 0,
                StartDate = p.StartDate ?? DateTime.Now,
                EndDate = p.EndDate ?? DateTime.Now.AddDays(7),
                Status = p.Status
            }).ToList();
        }

        public async Task<PromotionDTO?> GetPromotionByIdAsync(int id)
        {
            var p = await _promotionRepo.GetByIdAsync(id);
            if (p == null) return null;

            return new PromotionDTO
            {
                PromotionId = p.PromotionId,
                Code = p.Code,
                DiscountType = p.DiscountType,
                Value = p.Value,
                MinOrderValue = p.MinOrderValue ?? 0,
                StartDate = p.StartDate ?? DateTime.Now,
                EndDate = p.EndDate ??DateTime.Now.AddDays(7),
                Status = p.Status
            };
        }

        public async Task<bool> AddPromotionAsync(PromotionDTO dto)
        {
            if (dto.StartDate >= dto.EndDate) return false;

            var entity = new Promotion
            {
                Code = dto.Code,
                DiscountType = dto.DiscountType,
                Value = dto.Value,
                MinOrderValue = dto.MinOrderValue,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status
            };

            return await _promotionRepo.AddAsync(entity);
        }

        public async Task<bool> UpdatePromotionAsync(PromotionDTO dto)
        {
            if (dto.StartDate >= dto.EndDate) return false;

            var entity = await _promotionRepo.GetByIdAsync(dto.PromotionId);
            if (entity == null) return false;

            entity.Code = dto.Code;
            entity.DiscountType = dto.DiscountType;
            entity.Value = dto.Value;
            entity.MinOrderValue = dto.MinOrderValue;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.Status = dto.Status;

            return await _promotionRepo.UpdateAsync(entity);
        }

        public async Task<bool> DeletePromotionAsync(int id)
        {
            return await _promotionRepo.DeleteAsync(id);
        }
        public async Task<(bool Success, string Message, decimal DiscountAmount)> ValidatePromotionAsync(string code, decimal orderValue)
        {
            if (string.IsNullOrWhiteSpace(code))
                return (false, "Vui lòng nhập mã giảm giá.", 0);

            var all = await _promotionRepo.GetAllAsync(null, null);
            var promo = all.FirstOrDefault(p => p.Code.Equals(code, StringComparison.OrdinalIgnoreCase) && p.Status == "Active");

            if (promo == null)
                return (false, "Mã giảm giá không tồn tại hoặc đã bị vô hiệu hóa.", 0);

            if (promo.StartDate.HasValue && promo.StartDate > DateTime.Now)
                return (false, "Chương trình khuyến mãi chưa bắt đầu.", 0);

            if (promo.EndDate.HasValue && promo.EndDate < DateTime.Now)
                return (false, "Mã giảm giá đã hết hạn.", 0);

            if (orderValue < (promo.MinOrderValue ?? 0))
                return (false, $"Đơn hàng tối thiểu {(promo.MinOrderValue ?? 0):N0} xu mới được áp dụng mã này.", 0);

            decimal discount = promo.Value;

            discount = Math.Min(discount, orderValue);

            return (true, $"Áp dụng thành công mã '{promo.Code}'.", discount);
        }

     
    }
}
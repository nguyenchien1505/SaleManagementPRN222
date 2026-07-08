using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
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
            var promotions = await _promotionRepo.GetAllAsync(statusFilter, searchString);
            return promotions.Select(p => new PromotionDTO
            {
                PromotionId = p.PromotionId,
                Code = p.Code,
                DiscountType = p.DiscountType,
                Value = p.Value,
                MinOrderValue = p.MinOrderValue,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
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
                MinOrderValue = p.MinOrderValue,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
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
    }
}
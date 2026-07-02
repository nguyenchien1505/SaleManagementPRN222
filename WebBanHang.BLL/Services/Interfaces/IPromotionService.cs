using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IPromotionService
    {
        Task<IEnumerable<PromotionDTO>> GetAllPromotionsAsync(string? statusFilter, string? searchString);
        Task<PromotionDTO?> GetPromotionByIdAsync(int id);
        Task<bool> AddPromotionAsync(PromotionDTO dto);
        Task<bool> UpdatePromotionAsync(PromotionDTO dto);
        Task<bool> DeletePromotionAsync(int id);
    }
}
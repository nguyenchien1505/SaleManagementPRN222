using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IPromotionService
    {
        Task<IEnumerable<Promotion>> GetAllPromotionsAsync(string? statusFilter, string? searchString);
        Task<Promotion?> GetPromotionByIdAsync(int id);
        Task<bool> AddPromotionAsync(Promotion promotion);
        Task<bool> UpdatePromotionAsync(Promotion promotion);
        Task<bool> DeletePromotionAsync(int id);
        Task<(bool Success, string Message, decimal DiscountAmount)> ValidatePromotionAsync(string code, decimal orderValue);
    }
}

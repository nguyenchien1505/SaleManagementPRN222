using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface ICartService
    {
        Task<List<CartItemDTO>> GetCartByUserIdAsync(int userId);
        Task<CartItemDTO?> GetCartItemAsync(int userId, int productId);
        Task AddToCartAsync(int userId, int productId, int quantity);
        Task UpdateQuantityAsync(int userId, int productId, int quantity);
        Task RemoveFromCartAsync(int userId, int productId);
        Task ClearCartAsync(int userId);
        Task MergeCartAsync(int userId, List<CartItemDTO> sessionCart);
        Task<int> GetCartCountAsync(int userId);
    }
}

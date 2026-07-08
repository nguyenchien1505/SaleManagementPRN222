using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IOrderService
    {
<<<<<<< HEAD
        Task<(bool Success, string Message, int OrderId)> CheckoutAsync(int userId, int productId, int quantity);
        Task<(bool Success, string Message, int OrderId)> CheckoutCartAsync(int userId, List<(int productId, int quantity)> items,  string? promoCode = null);
        Task<IEnumerable<Order>> GetMyOrdersAsync(int userId);
        Task<bool> UpdateOrderStatusAsync(int id, string status);

        Task<Order?> GetOrderDetailsAsync(int id);
        Task<Order?> CreateOrderAsync(int customerId, List<int> productIds, List<int> quantities);
        Task<IEnumerable<Order>> GetOrdersOverviewAsync(string? searchString, string? statusFilter);
=======
        void ConfirmOrder(int orderId, int userId);
>>>>>>> haul
    }
}

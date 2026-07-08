using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Context;

namespace WebBanHang.BLL.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly WebBanHangContext _context;
        private readonly IInventoryService _inventoryService;

        public OrderService(WebBanHangContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public void ConfirmOrder(int orderId, int userId)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                throw new Exception("Đơn hàng không tồn tại");

            // Xuất kho: Tự động sinh khi Order Confirmed
            foreach (var detail in order.OrderDetails)
            {
                _inventoryService.XuatKho(
                    detail.ProductId,
                    detail.Quantity,
                    userId,
                    $"Tự động sinh khi Order Confirmed - Đơn hàng #{order.OrderCode}");
            }

            order.Status = "Confirmed";
            _context.Orders.Update(order);
            _context.SaveChanges();
        }
    }
}
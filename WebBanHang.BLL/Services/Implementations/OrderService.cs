using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Context;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.BLL.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly WebBanHangContext _context;
        private readonly IPromotionService _promotionService;

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository, WebBanHangContext context, IPromotionService promotionService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _context = context;
            _promotionService = promotionService;
        }

        public async Task<IEnumerable<Order>> GetMyOrdersAsync(int userId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null) return new List<Order>();

            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .Where(o => o.CustomerId == customer.CustomerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        public async Task<Order?> GetOrderDetailsAsync(int id)
        {
            return await _orderRepository.GetOrderDetailsAsync(id);
        }

        public async Task<(bool Success, string Message, int OrderId)> CheckoutAsync(int userId, int productId, int quantity)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
                    if (product == null) return (false, "Sản phẩm không tồn tại!", 0);

                    if (product.StockQuantity < quantity)
                        return (false, "Sản phẩm không đủ số lượng trong kho!", 0);

                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (customer == null)
                    {
                        return (false, "Bạn cần hoàn thiện hồ sơ khách hàng trước khi mua hàng!", 0);
                    }

                    product.StockQuantity -= quantity;

                    decimal subTotal = product.SellingPrice * quantity;

                    string uniqueOrderCode = $"ORD-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(100, 999)}";

                    var order = new Order
                    {
                        OrderCode = uniqueOrderCode, 
                        CustomerId = customer.CustomerId,
                        OrderDate = DateTime.Now,
                        SubTotal = subTotal,        
                        DiscountAmount = 0,
                        TotalAmount = subTotal,       
                        Status = "Confirmed",
                        CreatedBy = userId
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    var detail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = product.SellingPrice,
                        Total = subTotal
                    };
                    _context.OrderDetails.Add(detail);

                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ProductId = productId,
                        Quantity = -quantity,
                        CreatedDate = DateTime.Now,
                        Type = "DirectSale",
                        CreatedBy = userId
                    });

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Mua hàng thành công!", order.OrderId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Lỗi hệ thống: {ex.Message}", 0);
                }
            }
        }

        public async Task<(bool Success, string Message, int OrderId)> CheckoutCartAsync(int userId, List<(int productId, int quantity)> items, string? promoCode = null)
        {
            if (items == null || !items.Any())
                return (false, "Giỏ hàng trống!", 0);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (customer == null)
                        return (false, "Bạn cần hoàn thiện hồ sơ khách hàng trước khi mua hàng!", 0);

                    decimal totalOrderAmount = 0;
                    var processedItems = new List<(Product product, int quantity)>();

                    foreach (var item in items)
                    {
                        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == item.productId);
                        if (product == null)
                            return (false, $"Sản phẩm ID {item.productId} không tồn tại!", 0);

                        if (product.StockQuantity < item.quantity)
                            return (false, $"Sản phẩm '{product.Name}' không đủ số lượng trong kho (Còn {product.StockQuantity})!", 0);

                        totalOrderAmount += product.SellingPrice * item.quantity;
                        processedItems.Add((product, item.quantity));
                    }


                    decimal discountFromPromo = 0;
                    decimal finalPayableAmount = totalOrderAmount;

                    string uniqueOrderCode = $"ORD-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(100, 999)}";

                    var order = new Order
                    {
                        OrderCode = uniqueOrderCode,
                        CustomerId = customer.CustomerId,
                        OrderDate = DateTime.Now,
                        SubTotal = totalOrderAmount,       
                        DiscountAmount = discountFromPromo,
                        TotalAmount = finalPayableAmount,  // Tổng tiền phải trả sau cùng
                        Status = "Draft",                  // RÀNG BUỘC: Luôn luôn là Draft
                        CreatedBy = userId
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync(); 
                    foreach (var item in processedItems)
                    {
                        // Trừ kho của sản phẩm
                        item.product.StockQuantity -= item.quantity;

                        // Tạo Chi tiết đơn hàng
                        var detail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = item.product.ProductId,
                            Quantity = item.quantity,
                            UnitPrice = item.product.SellingPrice,
                            Total = item.product.SellingPrice * item.quantity
                        };
                        _context.OrderDetails.Add(detail);

                        // Ghi lịch sử giao dịch kho (Inventory log)
                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            ProductId = item.product.ProductId,
                            Quantity = -item.quantity,
                            CreatedDate = DateTime.Now,
                            Type = "Sale",
                            Note = $"Order Code: {order.OrderCode}",
                            CreatedBy = userId // Sử dụng chính userId mua hàng để khớp ràng buộc khóa ngoại trong DB
                        });
                    }

                    // 6. Lưu toàn bộ thay đổi dữ liệu chi tiết và commit giao dịch hoàn tất đặt đơn
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Đặt đơn hàng nháp thành công!", order.OrderId);
                }
                catch (Exception ex)
                {
                    // Hoàn tác dữ liệu cũ nếu xuất hiện lỗi bất kỳ
                    await transaction.RollbackAsync();
                    return (false, $"Lỗi hệ thống: {ex.Message}", 0);
                }
            }
        }
    }
}

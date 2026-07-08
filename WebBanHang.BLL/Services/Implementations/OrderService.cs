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
        public async Task<Order?> GetOrderDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User) 
                .Include(o => o.CreatedByNavigation) 
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product) 
                        .ThenInclude(p => p.ProductImages) 
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
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

                    if (!string.IsNullOrEmpty(promoCode) && promoCode.Trim() != "")
                    {
                        // 💡 SAU NÀY CÓ LOGIC PROMOTION BẠN CHỈ CẦN VIẾT VÀO ĐÂY:
                        // var promoResult = await _promotionService.ValidatePromotionAsync(promoCode, totalOrderAmount);
                        // if (promoResult.Success) {
                        //     discountFromPromo = promoResult.DiscountAmount;
                        // }

                        discountFromPromo = 0;
                    }

                    decimal finalPayableAmount = totalOrderAmount - discountFromPromo;
                    if (finalPayableAmount < 0) finalPayableAmount = 0;

                    string uniqueOrderCode = $"ORD-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(100, 999)}";

                    var order = new Order
                    {
                        OrderCode = uniqueOrderCode,
                        CustomerId = customer.CustomerId,
                        CreatedBy = userId,
                        OrderDate = DateTime.Now,
                        SubTotal = totalOrderAmount,
                        DiscountAmount = discountFromPromo,
                        TotalAmount = finalPayableAmount,
                        Status = "Draft" 
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync(); 

                    foreach (var item in processedItems)
                    {
                        item.product.StockQuantity -= item.quantity;

                        var detail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = item.product.ProductId,
                            Quantity = item.quantity,
                            UnitPrice = item.product.SellingPrice,
                            Total = item.product.SellingPrice * item.quantity
                        };
                        _context.OrderDetails.Add(detail);

                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            ProductId = item.product.ProductId,
                            Quantity = -item.quantity, 
                            CreatedDate = DateTime.Now,
                            Type = "Export",
                            Note = $"Order Code: {order.OrderCode}",
                            CreatedBy = userId 
                        });
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Đặt hàng thành công!", order.OrderId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    // 🔎 GIẢI PHẪU TẬN GỐC LỖI NGẦM CỦA SQL SERVER
                    string realSqlErrorMessage = ex.Message;

                    if (ex.InnerException != null)
                    {
                        realSqlErrorMessage = ex.InnerException.Message; // Tầng 1: EF Core Exception

                        if (ex.InnerException.InnerException != null)
                        {
                            realSqlErrorMessage = ex.InnerException.InnerException.Message; // Tầng 2: Lỗi gốc từ Microsoft.Data.SqlClient
                        }
                    }

                    // Trả về chuỗi lỗi thực sự từ SQL Server để hiển thị lên View
                    return (false, $"🚨 LỖI SQL THỰC TẾ: {realSqlErrorMessage}", 0);
                }
            }
        }
        public async Task<IEnumerable<Order>> GetOrdersOverviewAsync(string? searchString, string? statusFilter)
        {
            return await _orderRepository.GetOrdersWithCustomerAsync(searchString, statusFilter);
        }
        public async Task<Order?> CreateOrderAsync(int customerId, List<int> productIds, List<int> quantities)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = new Order
                    {
                        CustomerId = customerId,
                        OrderDate = DateTime.Now,
                        Status = "Completed",
                        CreatedBy = 1 
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    decimal subTotal = 0;

                    for (int i = 0; i < productIds.Count; i++)
                    {
                        var product = await _context.Products.FindAsync(productIds[i]);
                        if (product == null || quantities[i] <= 0 || quantities[i] > (product.StockQuantity))
                            continue;

                        var detail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = product.ProductId,
                            Quantity = quantities[i],
                            UnitPrice = product.SellingPrice,
                            Total = quantities[i] * product.SellingPrice
                        };

                        subTotal += detail.Total ;
                        product.StockQuantity -= quantities[i];
                        _context.OrderDetails.Add(detail);

                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            ProductId = product.ProductId,
                            Quantity = -quantities[i],
                            CreatedDate = DateTime.Now,
                            Type = "Sale",
                            CreatedBy = 1
                        });
                    }

                    order.SubTotal = subTotal;
                    order.TotalAmount = order.SubTotal;


                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return order;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return null;
                }
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return false;

            // 2. Cập nhật trạng thái đơn hàng ('Draft', 'Confirmed', 'Completed', 'Cancelled')
            order.Status = status;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

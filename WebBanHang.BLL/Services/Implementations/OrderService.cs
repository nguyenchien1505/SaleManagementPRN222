using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IInventoryService _inventoryService;
        private readonly WebBanHangContext _context;
        private readonly IPromotionService _promotionService;
        private readonly IAuditLogService _auditLogService;

        public OrderService(IInventoryService inventoryService, IOrderRepository orderRepository, IProductRepository productRepository, WebBanHangContext context, IPromotionService promotionService, IAuditLogService auditLogService)
        {
            _inventoryService = inventoryService;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _context = context;
            _promotionService = promotionService;
            _auditLogService = auditLogService;
        }

        public async Task<IEnumerable<Order>> GetMyOrdersAsync(int userId)
        {
            var customer = await _context.Users.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null) return new List<Order>();

            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .Where(o => o.CustomerId == customer.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderDetailsAsync(int orderId)
        {
            return await _context.Orders
                    .Include(c => c.Customer)
                .Include(o => o.CreatedByNavigation)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        // ──────────────────────────────────────────────────────────────────────
        // LUỒNG 1: MUA NGAY (Checkout nhanh 1 sản phẩm từ trang chi tiết)
        // ──────────────────────────────────────────────────────────────────────
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

                    var customer = await _context.Users.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (customer == null)
                        return (false, "Bạn cần hoàn thiện hồ sơ khách hàng trước khi mua hàng!", 0);

                    if (string.IsNullOrEmpty(customer.Address) || string.IsNullOrWhiteSpace(customer.Address))
                        return (false, "Đặt hàng thất bại! Vui lòng cập nhật địa chỉ giao hàng trong hồ sơ.", 0);

                    var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
                    int safeCreatedBy = userExists ? userId : (await _context.Users.Select(u => u.UserId).FirstOrDefaultAsync());

                    product.StockQuantity -= quantity;
                    decimal subTotal = product.SellingPrice * quantity;
                    string uniqueOrderCode = $"ORD-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(100, 999)}";

                    var order = new Order
                    {
                        OrderCode = uniqueOrderCode,
                        CustomerId = customer.UserId,
                        OrderDate = DateTime.Now,
                        SubTotal = subTotal,
                        DiscountAmount = 0,
                        TotalAmount = subTotal,
                        Status = "Draft",
                        CreatedBy = safeCreatedBy,

                        ShippingAddress = customer.Address,
                        ShippingPhone = customer.Phone
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
                        Type = "Export",
                        Note = $"Direct Order Code: {order.OrderCode}",
                        CreatedBy = safeCreatedBy
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

        // ──────────────────────────────────────────────────────────────────────
        // LUỒNG 2: ĐẶT HÀNG TỪ GIỎ HÀNG (CheckoutCart)
        // ──────────────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, int OrderId)> CheckoutCartAsync(int userId, List<(int productId, int quantity)> items, string? promoCode = null, bool isPayment = false)
        {
            if (items == null || !items.Any())
                return (false, "Giỏ hàng trống!", 0);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var customer = await _context.Users.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (customer == null)
                        return (false, "Bạn cần hoàn thiện hồ sơ khách hàng trước khi mua hàng!", 0);

                    if (string.IsNullOrEmpty(customer.Address) || string.IsNullOrWhiteSpace(customer.Address))
                    {
                        return (false, "Đặt hàng không thành công! Tài khoản của bạn chưa cập nhật địa chỉ giao hàng. Vui lòng bổ sung địa chỉ trong trang quản lý tài khoản để tiếp tục.", 0);
                    }

                    var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
                    int safeCreatedBy = userExists ? userId : (await _context.Users.Select(u => u.UserId).FirstOrDefaultAsync());

                    if (safeCreatedBy == 0)
                        return (false, "Hệ thống chưa có tài khoản User nào hợp lệ để tạo giao dịch kho!", 0);

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

                    // Tính toán tiền giảm giá thực tế từ mã Promotion được gửi lên
                    decimal discountFromPromo = 0;
                    if (!string.IsNullOrWhiteSpace(promoCode))
                    {
                        var promoResult = await _promotionService.ValidatePromotionAsync(promoCode, totalOrderAmount);
                        if (promoResult.Success)
                        {
                            discountFromPromo = promoResult.DiscountAmount;
                        }
                    }

                    // Tính số tiền cuối cùng khách cần trả sau khi trừ khuyến mãi
                    decimal finalPayableAmount = totalOrderAmount - discountFromPromo;
                    if (finalPayableAmount < 0) finalPayableAmount = 0;

                    string uniqueOrderCode = $"ORD-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(100, 999)}";

                    var order = new Order
                    {
                        OrderCode = uniqueOrderCode,
                        CustomerId = customer.UserId,
                        CreatedBy = safeCreatedBy,
                        OrderDate = DateTime.Now,
                        SubTotal = totalOrderAmount,      // Giá gốc trước giảm
                        DiscountAmount = discountFromPromo, // Lưu vết số tiền giảm giá
                        TotalAmount = finalPayableAmount,   // Giá thực tế phải thu
                        Status = "Draft",

                        ShippingAddress = customer.Address,
                        ShippingPhone = customer.Phone
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    foreach (var item in processedItems)
                    {
                        // 🌟 NẾU LÀ COD (isPayment = false): Trừ kho ngay lập tức
                        // 🌟 NẾU LÀ VNPAY (isPayment = true): BỎ QUA, không trừ kho ở bước này
                        if (!isPayment)
                        {
                            item.product.StockQuantity -= item.quantity;
                        }

                        var detail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = item.product.ProductId,
                            Quantity = item.quantity,
                            UnitPrice = item.product.SellingPrice,
                            Total = item.product.SellingPrice * item.quantity
                        };
                        _context.OrderDetails.Add(detail);

                        // 🌟 NẾU LÀ COD: Ghi nhận giao dịch xuất kho ngay
                        // 🌟 NẾU LÀ VNPAY: BỎ QUA, giao dịch kho sẽ được ghi nhận khi thanh toán thành công
                        if (!isPayment)
                        {
                            _context.InventoryTransactions.Add(new InventoryTransaction
                            {
                                ProductId = item.product.ProductId,
                                Quantity = -item.quantity,
                                CreatedDate = DateTime.Now,
                                Type = "Export",
                                Note = $"Order Code: {order.OrderCode}",
                                CreatedBy = safeCreatedBy
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Đặt hàng thành công!", order.OrderId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    string realSqlErrorMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        realSqlErrorMessage = ex.InnerException.Message;
                        if (ex.InnerException.InnerException != null)
                        {
                            realSqlErrorMessage = ex.InnerException.InnerException.Message;
                        }
                    }
                    return (false, $"Lỗi hệ thống: {ex.Message} | Chi tiết DB: {realSqlErrorMessage}", 0);
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

                        subTotal += detail.Total;
                        product.StockQuantity -= quantities[i];
                        _context.OrderDetails.Add(detail);

                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            ProductId = product.ProductId,
                            Quantity = -quantities[i],
                            CreatedDate = DateTime.Now,
                            Type = "Export",
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
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return false;

            if (order.Status == "Draft" && status == "Cancelled")
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == detail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += detail.Quantity;

                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            ProductId = detail.ProductId,
                            Quantity = detail.Quantity,
                            CreatedDate = DateTime.Now,
                            Type = "Import",
                            Note = $"Hoàn kho tự động - Hủy đơn hàng rác #{order.OrderCode}",
                            CreatedBy = order.CreatedBy
                        });
                    }
                }
            }

            order.Status = status;

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task UpdatePaymentInfoAsync(int orderId, string paymentMethod, string paymentStatus)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order != null)
            {
                order.PaymentMethod = paymentMethod;
                order.PaymentStatus = paymentStatus;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdatePaymentStatusAsync(int orderId, string paymentStatus)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order != null)
            {
                order.PaymentStatus = paymentStatus;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ConfirmOrderAndDeductStockAsync(int orderId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                        .FirstOrDefaultAsync(o => o.OrderId == orderId);

                    if (order == null || order.Status == "Confirmed") return false;

                    foreach (var detail in order.OrderDetails)
                    {
                        if (detail.Product.StockQuantity < detail.Quantity)
                            throw new Exception($"Sản phẩm '{detail.Product.Name}' không đủ số lượng trong kho!");

                        // Trừ kho thật
                        detail.Product.StockQuantity -= detail.Quantity;

                        // Ghi nhận lịch sử giao dịch kho
                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            ProductId = detail.ProductId,
                            Quantity = -detail.Quantity,
                            Type = "Export",
                            Note = $"Xuất kho thanh toán VNPay thành công. Đơn: {order.OrderCode}",
                            CreatedDate = DateTime.Now,
                            CreatedBy = order.CustomerId
                        });
                    }

                    order.Status = "Confirmed";
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }

        public async Task DeleteOrderIfFailedAsync(int orderId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.OrderDetails)
                        .FirstOrDefaultAsync(o => o.OrderId == orderId);

                    // Chỉ xóa nếu đơn hàng vẫn ở trạng thái Draft và chưa thanh toán Paid
                    if (order != null && order.Status == "Draft" && order.PaymentStatus != "Paid")
                    {
                        if (order.OrderDetails != null && order.OrderDetails.Any())
                        {
                            _context.OrderDetails.RemoveRange(order.OrderDetails);
                        }
                        _context.Orders.Remove(order);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                }
                catch
                {
                    await transaction.RollbackAsync();
                }
            }
        }
    }
}
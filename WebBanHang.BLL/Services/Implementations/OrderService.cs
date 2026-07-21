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
        private readonly IEmailService _emailService;

        public OrderService(IInventoryService inventoryService, IOrderRepository orderRepository, IProductRepository productRepository, WebBanHangContext context, IPromotionService promotionService, IAuditLogService auditLogService, IEmailService emailService)
        {
            _inventoryService = inventoryService;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _context = context;
            _promotionService = promotionService;
            _auditLogService = auditLogService;
            _emailService = emailService;
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

                    if (!_inventoryService.CanSell(productId, quantity))
                        return (false, "Sản phẩm không đủ số lượng trong kho!", 0);

                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (customer == null)
                        return (false, "Bạn cần hoàn thiện hồ sơ khách hàng trước khi mua hàng!", 0);

                    if (string.IsNullOrEmpty(customer.Address) || string.IsNullOrWhiteSpace(customer.Address))
                        return (false, "Đặt hàng thất bại! Vui lòng cập nhật địa chỉ giao hàng trong hồ sơ.", 0);

                    var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
                    int safeCreatedBy = userExists ? userId : (await _context.Users.Select(u => u.UserId).FirstOrDefaultAsync());

                    // ⚠️ Tồn kho KHÔNG bị trừ ở bước đặt hàng (Draft).
                    // Theo yêu cầu nghiệp vụ, Xuất kho chỉ được tự động sinh khi Order chuyển sang Confirmed
                    // (xem UpdateOrderStatusAsync / ConfirmOrder bên dưới).
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

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Email Notification: Đặt hàng thành công
                    var buyerUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                    if (!string.IsNullOrWhiteSpace(buyerUser?.Email))
                    {
                        await _emailService.SendOrderPlacedAsync(buyerUser.Email, buyerUser.FullName ?? "Quý khách", order.OrderCode, order.TotalAmount ?? 0);
                    }

                    return (true, "Mua hàng thành công! Đơn hàng đang chờ xác nhận.", order.OrderId);
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

                        if (!_inventoryService.CanSell(item.productId, item.quantity))
                            return (false, $"Sản phẩm '{product.Name}' không đủ số lượng trong kho (Còn {product.StockQuantity})!", 0);

                        totalOrderAmount += product.SellingPrice * item.quantity;
                        processedItems.Add((product, item.quantity));
                    }

                    // 🌟 ĐÃ SỬA: Tính toán tiền giảm giá thực tế từ mã Promotion được gửi lên
                    decimal discountFromPromo = 0;
                    if (!string.IsNullOrWhiteSpace(promoCode))
                    {
                        // Gọi qua PromotionService để kiểm tra tính hợp lệ và lấy số tiền giảm
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
                        CustomerId = customer.CustomerId,
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

                    // ⚠️ Tồn kho KHÔNG bị trừ ở bước đặt hàng (Draft).
                    // Theo yêu cầu nghiệp vụ, Xuất kho chỉ được tự động sinh khi Order chuyển sang Confirmed
                    // (xem UpdateOrderStatusAsync / ConfirmOrder bên dưới).
                    foreach (var item in processedItems)
                    {
                        var detail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = item.product.ProductId,
                            Quantity = item.quantity,
                            UnitPrice = item.product.SellingPrice,
                            Total = item.product.SellingPrice * item.quantity
                        };
                        _context.OrderDetails.Add(detail);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Email Notification: Đặt hàng thành công
                    var buyerUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                    if (!string.IsNullOrWhiteSpace(buyerUser?.Email))
                    {
                        await _emailService.SendOrderPlacedAsync(buyerUser.Email, buyerUser.FullName ?? "Quý khách", order.OrderCode, order.TotalAmount ?? 0);
                    }

                    return (true, "Đặt hàng thành công! Đơn hàng đang chờ xác nhận.", order.OrderId);
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

        // ──────────────────────────────────────────────────────────────────────
        // CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG
        // - Draft -> Confirmed : TỰ ĐỘNG XUẤT KHO (trừ tồn kho + ghi InventoryTransaction "Export").
        //   Không cho xác nhận nếu tồn kho hiện tại < số lượng đã đặt.
        // - Confirmed/Completed -> Cancelled : TỰ ĐỘNG HOÀN KHO (vì tồn đã bị trừ lúc Confirm).
        // - Draft -> Cancelled : không cần hoàn kho vì tồn chưa từng bị trừ.
        // ──────────────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(int id, string status, int? actorUserId = null)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.OrderDetails)
                            .ThenInclude(d => d.Product)
                        .Include(o => o.Customer)
                            .ThenInclude(c => c.User)
                        .FirstOrDefaultAsync(o => o.OrderId == id);

                    if (order == null)
                        return (false, "Đơn hàng không tồn tại!");

                    int effectiveUserId = (actorUserId.HasValue && actorUserId.Value > 0)
                        ? actorUserId.Value
                        : order.CreatedBy;

                    if (status == "Confirmed" && order.Status == "Draft")
                    {
                        // Xem tồn hiện tại: không cho xác nhận (bán) khi Quantity tồn < Quantity đã đặt
                        foreach (var detail in order.OrderDetails)
                        {
                            var product = detail.Product;
                            if (product == null || !_inventoryService.CanSell(detail.ProductId, detail.Quantity))
                            {
                                await transaction.RollbackAsync();
                                return (false, $"Không thể xác nhận đơn hàng: sản phẩm '{product?.Name ?? $"#{detail.ProductId}"}' không đủ tồn kho (còn {(product?.StockQuantity ?? 0)}, cần {detail.Quantity}).");
                            }
                        }

                        foreach (var detail in order.OrderDetails)
                        {
                            _inventoryService.XuatKho(
                                detail.ProductId,
                                detail.Quantity,
                                effectiveUserId,
                                $"Tự động sinh khi Order Confirmed - Đơn hàng #{order.OrderCode}");
                        }
                    }
                    else if (status == "Cancelled" && (order.Status == "Confirmed" || order.Status == "Completed"))
                    {
                        foreach (var detail in order.OrderDetails)
                        {
                            _inventoryService.NhapKho(
                                detail.ProductId,
                                detail.Quantity,
                                effectiveUserId,
                                $"Hoàn kho tự động - Hủy đơn hàng #{order.OrderCode}");
                        }
                    }
                    // Draft -> Cancelled: tồn kho chưa từng bị trừ nên không cần hoàn kho.

                    order.Status = status;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    if (status == "Confirmed")
                    {
                        // Audit Log: Ai xác nhận đơn
                        await _auditLogService.LogAsync("Order", order.OrderId, "Confirm", effectiveUserId, $"Xác nhận đơn hàng #{order.OrderCode}");

                        // Email Notification: Xác nhận đơn hàng
                        var email = order.Customer?.User?.Email;
                        var fullName = order.Customer?.User?.FullName ?? "Quý khách";
                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            await _emailService.SendOrderConfirmedAsync(email, fullName, order.OrderCode);
                        }
                    }

                    return (true, "Cập nhật trạng thái đơn hàng thành công!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Lỗi hệ thống: {ex.Message}");
                }
            }
        }

        // Xác nhận đơn hàng (Draft -> Confirmed) và tự động sinh giao dịch Xuất kho.
        // Được giữ lại cho các luồng gọi trực tiếp (vd. API/nội bộ); tái sử dụng cùng quy tắc
        // với UpdateOrderStatusAsync để tránh trừ kho 2 lần cho cùng một đơn hàng.
        public void ConfirmOrder(int orderId, int userId)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                throw new Exception("Đơn hàng không tồn tại");

            if (order.Status != "Draft")
                throw new InvalidOperationException("Chỉ có thể xác nhận đơn hàng đang ở trạng thái 'Draft'.");

            foreach (var detail in order.OrderDetails)
            {
                var product = detail.Product;
                if (product == null || !_inventoryService.CanSell(detail.ProductId, detail.Quantity))
                    throw new InvalidOperationException($"Không đủ tồn kho để xác nhận đơn hàng cho sản phẩm '{product?.Name}'.");
            }

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
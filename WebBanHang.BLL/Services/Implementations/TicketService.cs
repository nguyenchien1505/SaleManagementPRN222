using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Context;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Implementations
{
    public class TicketService : ITicketService
    {
        private readonly WebBanHangContext _context;

        public TicketService(WebBanHangContext context)
        {
            _context = context;
        }

        // ──────────────────────────────────────────────────────────────
        // CUSTOMER: Tạo ticket
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, int TicketId)> CreateTicketAsync(int customerId, CreateTicketDTO dto)
        {
            if (dto.TicketType != "WarrantyRequest" && dto.TicketType != "ReturnRequest")
                return (false, "Loại yêu cầu không hợp lệ.", 0);

            var orderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .FirstOrDefaultAsync(od => od.OrderDetailId == dto.OrderDetailId);

            if (orderDetail == null)
                return (false, "Không tìm thấy sản phẩm trong đơn hàng.", 0);

            if (orderDetail.Order.CustomerId != customerId)
                return (false, "Bạn không có quyền tạo yêu cầu cho đơn hàng này.", 0);

            if (orderDetail.Order.Status != "Completed")
                return (false, "Chỉ có thể yêu cầu bảo hành/đổi trả cho đơn hàng đã hoàn tất.", 0);

            bool hasOpenTicket = await _context.SupportTickets.AnyAsync(t =>
                t.OrderDetailId == dto.OrderDetailId &&
                t.Status != "Closed" && t.Status != "Cancelled" && t.Status != "Rejected");
            if (hasOpenTicket)
                return (false, "Sản phẩm này đã có yêu cầu CSKH đang xử lý.", 0);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ticket = new SupportTicket
                {
                    TicketCode = $"TK-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(100, 999)}",
                    TicketType = dto.TicketType,
                    OrderId = orderDetail.OrderId,
                    OrderDetailId = dto.OrderDetailId,
                    CustomerId = customerId,
                    Status = "New",
                    Description = dto.Description,
                    CreatedDate = DateTime.Now
                };
                _context.SupportTickets.Add(ticket);
                await _context.SaveChangesAsync();

                if (dto.AttachmentUrls != null)
                {
                    foreach (var url in dto.AttachmentUrls)
                    {
                        _context.TicketAttachments.Add(new TicketAttachment
                        {
                            TicketId = ticket.TicketId,
                            FileUrl = url,
                            UploadedDate = DateTime.Now
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return (true, "Tạo yêu cầu CSKH thành công.", ticket.TicketId);
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

        public async Task<IEnumerable<TicketListItemDTO>> GetMyTicketsAsync(int customerId)
        {
            return await BuildListQuery(_context.SupportTickets.Where(t => t.CustomerId == customerId));
        }

        public async Task<bool> CancelTicketAsync(int ticketId, int customerId)
        {
            var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == ticketId);
            if (ticket == null || ticket.CustomerId != customerId) return false;

            if (ticket.Status != "New" && ticket.Status != "InProgress" && ticket.Status != "WaitingCustomer")
                return false;

            ticket.Status = "Cancelled";
            ticket.ClosedDate = DateTime.Now;
            ticket.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        // ──────────────────────────────────────────────────────────────
        // DÙNG CHUNG
        // ──────────────────────────────────────────────────────────────
        public async Task<SupportTicket?> GetTicketDetailAsync(int ticketId)
        {
            return await _context.SupportTickets
                .Include(t => t.Order)
                .Include(t => t.OrderDetail).ThenInclude(od => od.Product)
                .Include(t => t.Customer)
                .Include(t => t.AssignedSale)
                .Include(t => t.ApprovedByUser)
                .Include(t => t.TicketAttachments)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }

        // ──────────────────────────────────────────────────────────────
        // SALE
        // ──────────────────────────────────────────────────────────────
        public async Task<IEnumerable<TicketListItemDTO>> GetSaleQueueAsync(string? statusFilter)
        {
            var query = _context.SupportTickets.AsQueryable();
            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(t => t.Status == statusFilter);
            else
                query = query.Where(t => t.Status != "Closed" && t.Status != "Cancelled");

            return await BuildListQuery(query);
        }

        public async Task<bool> StartProcessingAsync(int ticketId, int saleId)
        {
            var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == ticketId);
            if (ticket == null || ticket.Status != "New") return false;

            ticket.AssignedSaleId = saleId;
            ticket.Status = "InProgress";
            ticket.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmitForApprovalAsync(int saleId, SubmitForApprovalDTO dto)
        {
            if (dto.ResolutionType != "Refund" && dto.ResolutionType != "Exchange" && dto.ResolutionType != "Repair")
                return false;

            var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == dto.TicketId);
            if (ticket == null || ticket.Status != "InProgress") return false;
            if (ticket.AssignedSaleId != saleId) return false;

            ticket.ResolutionType = dto.ResolutionType;
            ticket.RefundAmount = dto.RefundAmount;
            ticket.Status = "PendingApproval";
            ticket.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message)> CompleteTicketAsync(int ticketId, int saleId)
        {
            var ticket = await _context.SupportTickets
                .Include(t => t.OrderDetail)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null) return (false, "Không tìm thấy ticket.");
            if (ticket.Status != "Approved") return (false, "Ticket chưa được Manager duyệt.");
            if (ticket.AssignedSaleId != saleId) return (false, "Bạn không phải Sale phụ trách ticket này.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == ticket.OrderDetail.ProductId);
                int qty = ticket.OrderDetail.Quantity;

                if (ticket.ResolutionType == "Exchange")
                {
                    if (product == null) return (false, "Sản phẩm không còn tồn tại.");

                    if (ticket.TicketType == "WarrantyRequest")
                    {
                        // Hàng lỗi bảo hành: KHÔNG cộng lại vào kho bán được (hàng hỏng, không resale).
                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            ProductId = product.ProductId,
                            Quantity = 0,
                            Type = "Adjustment",
                            Note = $"Nhận hàng lỗi bảo hành (không nhập lại kho bán) - Ticket {ticket.TicketCode}",
                            CreatedDate = DateTime.Now,
                            CreatedBy = saleId
                        });

                        if (product.StockQuantity < qty)
                            return (false, $"Không đủ hàng trong kho để đổi (còn {product.StockQuantity}).");

                        product.StockQuantity -= qty;
                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            ProductId = product.ProductId,
                            Quantity = -qty,
                            Type = "Export",
                            Note = $"Xuất hàng thay thế bảo hành - Ticket {ticket.TicketCode}",
                            CreatedDate = DateTime.Now,
                            CreatedBy = saleId
                        });
                    }
                    else // ReturnRequest - hàng còn tốt, resale được
                    {
                        product.StockQuantity += qty;
                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            ProductId = product.ProductId,
                            Quantity = qty,
                            Type = "Import",
                            Note = $"Nhận hàng trả lại (còn tốt) - Ticket {ticket.TicketCode}",
                            CreatedDate = DateTime.Now,
                            CreatedBy = saleId
                        });

                        if (product.StockQuantity < qty)
                            return (false, $"Không đủ hàng trong kho để đổi (còn {product.StockQuantity}).");

                        product.StockQuantity -= qty;
                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            ProductId = product.ProductId,
                            Quantity = -qty,
                            Type = "Export",
                            Note = $"Xuất hàng đổi mới - Ticket {ticket.TicketCode}",
                            CreatedDate = DateTime.Now,
                            CreatedBy = saleId
                        });
                    }
                }
                else if (ticket.ResolutionType == "Refund" && ticket.TicketType == "ReturnRequest" && product != null)
                {
                    product.StockQuantity += qty;
                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ProductId = product.ProductId,
                        Quantity = qty,
                        Type = "Import",
                        Note = $"Nhận hàng trả lại (hoàn tiền) - Ticket {ticket.TicketCode}",
                        CreatedDate = DateTime.Now,
                        CreatedBy = saleId
                    });
                }
                // Refund cho WarrantyRequest hoặc Repair: không tác động kho

                ticket.Status = "Resolved";
                ticket.ClosedDate = DateTime.Now;
                ticket.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Hoàn tất xử lý ticket.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────────────────────
        // MANAGER
        // ──────────────────────────────────────────────────────────────
        public async Task<IEnumerable<TicketListItemDTO>> GetPendingApprovalAsync()
        {
            return await BuildListQuery(_context.SupportTickets.Where(t => t.Status == "PendingApproval"));
        }

        public async Task<bool> ApproveAsync(int managerId, ApproveTicketDTO dto)
        {
            bool isManager = await _context.Users.AnyAsync(u => u.UserId == managerId && u.Role == "Manager");
            if (!isManager) return false;

            var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == dto.TicketId);
            if (ticket == null || ticket.Status != "PendingApproval") return false;

            ticket.ApprovedByUserId = managerId;
            ticket.ApprovedDate = DateTime.Now;
            ticket.Status = "Approved";
            ticket.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(int managerId, RejectTicketDTO dto)
        {
            bool isManager = await _context.Users.AnyAsync(u => u.UserId == managerId && u.Role == "Manager");
            if (!isManager) return false;

            var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == dto.TicketId);
            if (ticket == null || ticket.Status != "PendingApproval") return false;

            ticket.ApprovedByUserId = managerId;
            ticket.RejectReason = dto.RejectReason;
            ticket.ClosedDate = DateTime.Now;
            ticket.Status = "Rejected";
            ticket.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        // ──────────────────────────────────────────────────────────────
        // HELPER
        // ──────────────────────────────────────────────────────────────
        private async Task<IEnumerable<TicketListItemDTO>> BuildListQuery(IQueryable<SupportTicket> query)
        {
            return await query
                .OrderByDescending(t => t.CreatedDate)
                .Select(t => new TicketListItemDTO
                {
                    TicketId = t.TicketId,
                    TicketCode = t.TicketCode,
                    TicketType = t.TicketType,
                    Status = t.Status,
                    OrderCode = t.Order.OrderCode,
                    ProductName = t.OrderDetail.Product.Name,
                    CustomerName = t.Customer.FullName,
                    CreatedDate = t.CreatedDate
                })
                .ToListAsync();
        }
    }
}
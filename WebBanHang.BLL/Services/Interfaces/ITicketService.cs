using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Interfaces
{

    public interface ITicketService
    {
        // Customer
        Task<(bool Success, string Message, int TicketId)> CreateTicketAsync(int customerId, CreateTicketDTO dto);
        Task<IEnumerable<TicketListItemDTO>> GetMyTicketsAsync(int customerId);
        Task<bool> CancelTicketAsync(int ticketId, int customerId);

        // Dùng chung
        Task<SupportTicket?> GetTicketDetailAsync(int ticketId);

        // Sale
        Task<IEnumerable<TicketListItemDTO>> GetSaleQueueAsync(string? statusFilter);
        Task<bool> StartProcessingAsync(int ticketId, int saleId);
        Task<bool> SubmitForApprovalAsync(int saleId, SubmitForApprovalDTO dto);
        Task<(bool Success, string Message)> CompleteTicketAsync(int ticketId, int saleId);

        // Manager
        Task<IEnumerable<TicketListItemDTO>> GetPendingApprovalAsync();
        Task<bool> ApproveAsync(int managerId, ApproveTicketDTO dto);
        Task<bool> RejectAsync(int managerId, RejectTicketDTO dto);
    }
}

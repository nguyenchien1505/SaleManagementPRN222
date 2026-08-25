using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.BLL.DTOs
{
    public class CreateTicketDTO
    {
        public int OrderDetailId { get; set; }
        public string TicketType { get; set; } = null!;   // WarrantyRequest | ReturnRequest
        public string Description { get; set; } = null!;  
        public List<string>? AttachmentUrls { get; set; }  
    }

    // Sale đề xuất hướng xử lý, chuyển ticket lên chờ Manager duyệt
    public class SubmitForApprovalDTO
    {
        public int TicketId { get; set; }
        public string ResolutionType { get; set; } = null!; // Refund | Exchange | Repair
        public decimal? RefundAmount { get; set; }
    }

    // Manager duyệt / từ chối
    public class ApproveTicketDTO
    {
        public int TicketId { get; set; }
    }

    public class RejectTicketDTO
    {
        public int TicketId { get; set; }
        public string RejectReason { get; set; } = null!;
    }

    // Hiển thị danh sách (Sale/Manager/Customer)
    public class TicketListItemDTO
    {
        public int TicketId { get; set; }
        public string TicketCode { get; set; } = null!;
        public string TicketType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string OrderCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public DateTime? CreatedDate { get; set; }
    }
}

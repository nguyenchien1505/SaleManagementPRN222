using System;
using System.Collections.Generic;

namespace WebBanHang.DAL.Entities;

public partial class SupportTicket
{
    public int TicketId { get; set; }

    public string TicketCode { get; set; } = null!;

    public string TicketType { get; set; } = null!;

    public int OrderId { get; set; }

    public int OrderDetailId { get; set; }

    public int CustomerId { get; set; }

    public int? AssignedSaleId { get; set; }

    public int? ApprovedByUserId { get; set; }

    public string Status { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? ResolutionType { get; set; }

    public decimal? RefundAmount { get; set; }

    public string? RejectReason { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public DateTime? ClosedDate { get; set; }

    public virtual User? ApprovedByUser { get; set; }

    public virtual User? AssignedSale { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual OrderDetail OrderDetail { get; set; } = null!;

    public virtual ICollection<TicketAttachment> TicketAttachments { get; set; } = new List<TicketAttachment>();
}

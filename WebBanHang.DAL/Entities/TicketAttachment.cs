using System;
using System.Collections.Generic;

namespace WebBanHang.DAL.Entities;

public partial class TicketAttachment
{
    public int AttachmentId { get; set; }

    public int TicketId { get; set; }

    public string FileUrl { get; set; } = null!;

    public string? FileType { get; set; }

    public DateTime? UploadedDate { get; set; }

    public virtual SupportTicket Ticket { get; set; } = null!;
}

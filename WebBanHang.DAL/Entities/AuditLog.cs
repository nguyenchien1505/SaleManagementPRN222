using System;
using System.Collections.Generic;

namespace WebBanHang.DAL.Entities;

public partial class AuditLog
{
    public int AuditLogId { get; set; }

    public string EntityName { get; set; } = null!;

    public int? EntityId { get; set; }

    public string Action { get; set; } = null!;

    public int? PerformedBy { get; set; }

    public string? PerformedByName { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? PerformedByRole { get; set; }

    public string? IpAddress { get; set; }

    public string? RequestPath { get; set; }
}

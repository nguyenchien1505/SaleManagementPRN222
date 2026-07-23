using System;
using System.Collections.Generic;

namespace WebBanHang.DAL.Entities;

public partial class AuditLog
{
    public int AuditLogId { get; set; }

    public string EntityName { get; set; } = null!;

    public int EntityId { get; set; }

    public string Action { get; set; } = null!;

    public int PerformedBy { get; set; }

    public string? PerformedByName { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }
}

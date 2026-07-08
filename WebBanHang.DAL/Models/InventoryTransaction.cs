using System;
using System.Collections.Generic;

namespace WebBanHang.DAL.Models;

public partial class InventoryTransaction
{
    public int TransactionId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public string? Type { get; set; }

    public string? Note { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}

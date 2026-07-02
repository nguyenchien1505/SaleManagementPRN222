using System;
using System.Collections.Generic;

namespace WebBanHang.DAL.Entities;

public partial class Order
{
    public int OrderId { get; set; }

    public string OrderCode { get; set; }

    public int CustomerId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal? SubTotal { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Status { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<OrderPromotion> OrderPromotions { get; set; } = new List<OrderPromotion>();
}

using System;
using System.Collections.Generic;

namespace WebBanHang.DAL.Entities;

public partial class Order
{
    public int OrderId { get; set; }

    public string OrderCode { get; set; } = null!;

    public int CustomerId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal? SubTotal { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Status { get; set; }

    public string? ShippingAddress { get; set; }

    public string? ShippingPhone { get; set; }
}

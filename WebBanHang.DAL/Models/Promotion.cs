using System;
using System.Collections.Generic;

namespace WebBanHang.DAL.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string Code { get; set; } = null!;

    public string? DiscountType { get; set; }

    public decimal Value { get; set; }

    public decimal? MinOrderValue { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<OrderPromotion> OrderPromotions { get; set; } = new List<OrderPromotion>();
}

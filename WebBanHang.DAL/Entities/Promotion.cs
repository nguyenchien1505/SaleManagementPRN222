using System;
using System.Collections.Generic;

namespace WebBanHang.DAL.Entities
{
    public class Promotion
    {
        public int PromotionId { get; set; }
        public string Code { get; set; } = null!;
        public string DiscountType { get; set; } = null!;
        public decimal Value { get; set; }
        public decimal MinOrderValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Active";

        // Đồng bộ mối quan hệ thực tế trong WebBanHangContext
        public virtual ICollection<OrderPromotion> OrderPromotions { get; set; } = new List<OrderPromotion>();
    }
}
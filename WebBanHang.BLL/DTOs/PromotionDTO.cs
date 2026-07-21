using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.BLL.DTOs
{
    public class PromotionDTO
    {
        public int PromotionId { get; set; }
        public string Code { get; set; } = null!;
        public string DiscountType { get; set; } = null!;
        public decimal Value { get; set; }
        public decimal MinOrderValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!;
    }
}
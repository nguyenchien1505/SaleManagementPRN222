using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.DAL.QueryModels.Dashboard
{
    public class TopProductResult
    {
        public string ProductName { get; set; } = "";
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class MonthlyOrderCountResult
    {
        public int Month { get; set; }
        public int Count { get; set; }
    }
}

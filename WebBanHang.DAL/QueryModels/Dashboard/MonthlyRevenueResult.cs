using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.DAL.QueryModels.Dashboard
{
    public class MonthlyRevenueResult
    {
        public int Month { get; set; }

        public decimal TotalRevenue { get; set; }
    }
}

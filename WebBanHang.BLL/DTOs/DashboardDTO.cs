using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.BLL.DTOs
{
    public class DashboardDTO
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int NewUsers { get; set; }
        public List<RecentOrderDTO> RecentOrders { get; set; } = new List<RecentOrderDTO>();

        // Data for charts
        public List<decimal> RevenueData { get; set; } = new List<decimal>(); // 12 months
        public List<string> CategoryLabels { get; set; } = new List<string>();
        public List<int> CategoryData { get; set; } = new List<int>();
    }
}

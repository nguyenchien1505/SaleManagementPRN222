using WebBanHang.BLL.DTOs;
using WebBanHang.DAL.Entities;

namespace WebBanHang.ViewModels
{
    public class AdminDashboardVM
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int NewUsers { get; set; }

        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int DraftOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int CompletedOrders { get; set; }

        public List<RecentOrderDTO> RecentOrders { get; set; } = new List<RecentOrderDTO>();

        public List<decimal> RevenueData { get; set; } = new List<decimal>();
        public List<int> OrderCountData { get; set; } = new List<int>();
        public List<TopProductDTO> TopProducts { get; set; } = new List<TopProductDTO>();

        public List<string> CategoryLabels { get; set; } = new List<string>();
        public List<int> CategoryData { get; set; } = new List<int>();
    }
}

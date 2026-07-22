using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;

public class DashboardDTO
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int NewUsers { get; set; }

    // MỚI: bổ sung KPI còn thiếu
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }

    // MỚI: thống kê đơn theo trạng thái
    public int DraftOrders { get; set; }
    public int ConfirmedOrders { get; set; }
    public int CompletedOrders { get; set; }

    public List<RecentOrderDTO> RecentOrders { get; set; } = new List<RecentOrderDTO>();

    // Data for charts
    public List<decimal> RevenueData { get; set; } = new List<decimal>(); // 12 months

    // MỚI: Order Chart - số đơn theo tháng
    public List<int> OrderCountData { get; set; } = new List<int>();

    // MỚI: Top sản phẩm bán chạy (thay cho Category chart)
    public List<TopProductDTO> TopProducts { get; set; } = new List<TopProductDTO>();

    // Giữ lại nếu bạn vẫn muốn hiện Top danh mục ở chỗ khác
    public List<string> CategoryLabels { get; set; } = new List<string>();
    public List<int> CategoryData { get; set; } = new List<int>();
}

public class TopProductDTO
{
    public string ProductName { get; set; } = "";
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

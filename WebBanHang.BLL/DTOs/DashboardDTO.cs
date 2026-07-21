using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.BLL.DTOs
{
    // KPI tổng quan (Overview) hiển thị ở 4 thẻ đầu Dashboard
    public class DashboardOverviewDTO
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }

        // Thống kê đơn hàng theo trạng thái: Draft / Confirmed / Completed / Cancelled
        public int DraftOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }

        // Top sản phẩm bán chạy: Tên sản phẩm / Số lượng bán / Doanh thu
        public List<TopProductDTO> TopProducts { get; set; } = new List<TopProductDTO>();
    }

    public class TopProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    // Thống kê doanh thu theo Ngày / Tháng / Năm (bảng có thể chuyển đổi)
    public class RevenueStatDTO
    {
        public string Label { get; set; } = string.Empty; // vd: 17/07/2026, Thg 07/2026, Năm 2026
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    // Dữ liệu cho 3 biểu đồ: Revenue Chart, Order Chart, Product Chart
    public class DashboardChartDTO
    {
        public List<string> MonthLabels { get; set; } = new List<string>();
        public List<decimal> RevenueByMonth { get; set; } = new List<decimal>();
        public List<int> OrderCountByMonth { get; set; } = new List<int>();

        public List<string> TopProductNames { get; set; } = new List<string>();
        public List<int> TopProductQuantities { get; set; } = new List<int>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IDashboardService
    {
        // KPI + thống kê trạng thái đơn hàng + top sản phẩm bán chạy
        Task<DashboardOverviewDTO> GetOverviewAsync();

        // Thống kê doanh thu theo Ngày / Tháng / Năm. period = "day" | "month" | "year"
        Task<List<RevenueStatDTO>> GetRevenueStatsAsync(string period);

        // Dữ liệu cho Revenue Chart, Order Chart, Product Chart
        Task<DashboardChartDTO> GetChartDataAsync();
    }
}

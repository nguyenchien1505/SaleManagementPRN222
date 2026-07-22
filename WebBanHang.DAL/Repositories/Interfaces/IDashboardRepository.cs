using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.QueryModels.Dashboard;

namespace WebBanHang.DAL.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<decimal> GetTotalRevenueAsync();

        Task<int> GetTotalOrdersAsync();

        Task<int> GetPendingOrdersAsync();

        Task<int> GetNewUsersAsync(int month, int year);

        Task<List<RecentOrderResult>> GetRecentOrdersAsync(int count);

        Task<List<MonthlyRevenueResult>> GetMonthlyRevenueAsync(int year);

        Task<List<CategoryStatisticResult>> GetTopCategoriesAsync(int count);

        Task<int> GetTotalProductsAsync();
        Task<int> GetTotalCustomersAsync();

        Task<int> GetOrderCountByStatusAsync(string status);

        Task<List<MonthlyOrderCountResult>> GetMonthlyOrderCountAsync(int year);

        Task<List<TopProductResult>> GetTopProductsAsync(int count);
    }
}

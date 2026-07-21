using WebBanHang.DAL.Entities;

namespace WebBanHang.ViewModels
{
    public class ReportVM
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public int DraftOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }

        public List<ReportTopProductVM> TopProducts { get; set; } = new();
        public List<Order> Orders { get; set; } = new();

        // Pagination
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }

    public class ReportTopProductVM
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }
}

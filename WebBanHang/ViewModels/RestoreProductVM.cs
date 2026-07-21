namespace WebBanHang.ViewModels
{
    public class RestoreProductVM
    {
        public int ProductId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public decimal SellingPrice { get; set; }
        public int StockQuantity { get; set; }
    }
}

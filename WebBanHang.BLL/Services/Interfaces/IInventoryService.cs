using System.Collections.Generic;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IInventoryService
    {
        void NhapKho(int productId, int quantity, int userId, string? supplierName = null, string? note = null);

        void XuatKho(int productId, int quantity, int userId, string? note = null);

        List<InventoryTransaction> GetHistory();

        List<InventoryTransaction> GetHistoryByProduct(int productId);

        Product? GetStock(int productId);

        bool CanSell(int productId, int quantityOrdered);

        List<Product> GetLowStockWarnings();
        Task<int> ImportInboundFromExcelAsync(Stream fileStream, int userId);
    }
}
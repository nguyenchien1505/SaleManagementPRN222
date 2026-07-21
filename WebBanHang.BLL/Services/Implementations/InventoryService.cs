using System;
using System.Collections.Generic;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.BLL.Services.Implementations
{
    public class InventoryService : IInventoryService
    {
        private const int LOW_STOCK_THRESHOLD = 10;

        private readonly IInventoryRepository _repo;

        public InventoryService(IInventoryRepository repo)
        {
            _repo = repo;
        }

        public void NhapKho(int productId, int quantity, int userId, string? note = null)
        {
            if (quantity <= 0)
                throw new ArgumentException("Số lượng nhập phải lớn hơn 0");

            var product = _repo.GetProductWithStock(productId);
            if (product == null)
                throw new Exception("Sản phẩm không tồn tại");

            product.StockQuantity += quantity;
            _repo.UpdateProductStock(product);

            _repo.AddTransaction(new InventoryTransaction
            {
                ProductId = productId,
                Quantity = quantity,
                Type = "Import",
                Note = note,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            });
        }

        public void XuatKho(int productId, int quantity, int userId, string? note = null)
        {
            if (quantity <= 0)
                throw new ArgumentException("Số lượng xuất phải lớn hơn 0");

            var product = _repo.GetProductWithStock(productId);
            if (product == null)
                throw new Exception("Sản phẩm không tồn tại");

            if (product.StockQuantity < quantity)
                throw new InvalidOperationException($"Không đủ tồn kho để xuất sản phẩm '{product.Name}'");

            product.StockQuantity -= quantity;
            _repo.UpdateProductStock(product);

            _repo.AddTransaction(new InventoryTransaction
            {
                ProductId = productId,
                Quantity = quantity,
                Type = "Export",
                Note = note ?? "Tự động sinh khi Order Confirmed",
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            });
        }

        public List<InventoryTransaction> GetHistory() => _repo.GetAll();

        public List<InventoryTransaction> GetHistoryByProduct(int productId)
            => _repo.GetTransactionsByProduct(productId);

        public Product? GetStock(int productId) => _repo.GetProductWithStock(productId);

        public bool CanSell(int productId, int quantityOrdered)
        {
            var product = _repo.GetProductWithStock(productId);
            if (product == null) return false;
            return product.StockQuantity >= quantityOrdered;
        }

        public List<Product> GetLowStockWarnings()
            => _repo.GetLowStockProducts(LOW_STOCK_THRESHOLD);
    }
}
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Implementations;
using WebBanHang.DAL.Repositories.Interfaces;
using OfficeOpenXml;
using System.IO;
namespace WebBanHang.BLL.Services.Implementations
{
    public class InventoryService : IInventoryService
    {
        private const int LOW_STOCK_THRESHOLD = 10;

        private readonly IInventoryRepository _repo;
        private readonly IProductRepository _productRepository;
        public InventoryService(IProductRepository productRepository, IInventoryRepository repo)
        {
            _repo = repo;
            _productRepository = productRepository;
        }

        public void NhapKho(int productId, int quantity, int userId, string? supplierName = null, string? note = null)
        {
            if (quantity <= 0)
                throw new ArgumentException("Số lượng nhập phải lớn hơn 0");

            var product = _repo.GetProductWithStock(productId);
            if (product == null)
                throw new Exception("Sản phẩm không tồn tại");

            product.StockQuantity += quantity;

            if (!string.IsNullOrWhiteSpace(supplierName))
            {
                product.SupplierName = supplierName.Trim();
            }

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
        public async Task<int> ImportInboundFromExcelAsync(Stream fileStream, int userId)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Admin");
            int successCount = 0;

            using (var package = new ExcelPackage(fileStream))
            {
                if (package.Workbook.Worksheets.Count == 0)
                    throw new Exception("File Excel trống, không có dữ liệu.");

                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension?.Rows ?? 0;
                int colCount = worksheet.Dimension?.Columns ?? 0;

                if (colCount < 2)
                {
                    throw new Exception("File Excel sai biểu mẫu. Biểu mẫu nhập kho phải có ít nhất 2 cột (Cột A: Mã sản phẩm, Cột B: Số lượng).");
                }

                var col1Header = worksheet.Cells[1, 1].Value?.ToString()?.Trim().ToLower();
                var col2Header = worksheet.Cells[1, 2].Value?.ToString()?.Trim().ToLower();

                if (col1Header != "mã sản phẩm" || col2Header != "số lượng nhập")
                {
                    throw new Exception("File Excel sai cấu trúc tiêu đề (Cột A phải là 'Mã Sản Phẩm', Cột B phải là 'Số Lượng Nhập').");
                }

                if (rowCount < 2)
                    throw new Exception("File Excel không có dòng dữ liệu nào để nhập.");

                for (int row = 2; row <= rowCount; row++)
                {
                    var productCode = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var qtyStr = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    var note = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "Nhập kho hàng loạt qua Excel";
                    var supplierName = colCount >= 4
                        ? worksheet.Cells[row, 4].Value?.ToString()?.Trim()
                        : null;
                    if (string.IsNullOrEmpty(supplierName)) supplierName = null;

                    if (string.IsNullOrEmpty(productCode) && string.IsNullOrEmpty(qtyStr)) continue;

                    if (string.IsNullOrEmpty(productCode))
                    {
                        throw new Exception($"Dòng {row}: Mã sản phẩm không được để trống.");
                    }

                    if (!int.TryParse(qtyStr, out int quantity) || quantity <= 0)
                    {
                        throw new Exception($"Dòng {row} (Mã SP: {productCode}): Số lượng nhập phải là số nguyên lớn hơn 0.");
                    }
                    if (_productRepository == null)
                    {
                        throw new Exception("Lỗi hệ thống: IProductRepository chưa được khởi tạo (Inject).");
                    }

                    var product = _productRepository.GetAll()
                        .FirstOrDefault(p => p.Code.Equals(productCode, StringComparison.OrdinalIgnoreCase));

                    if (product == null)
                    {
                        throw new Exception($"Dòng {row}: Mã sản phẩm '{productCode}' không tồn tại trong hệ thống! Vui lòng kiểm tra lại.");
                    }

                    NhapKho(product.ProductId, quantity, userId, supplierName, note);
                    successCount++;
                }
            }

            return successCount;
        }
    }
}
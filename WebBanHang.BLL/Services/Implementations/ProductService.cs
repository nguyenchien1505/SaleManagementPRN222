using OfficeOpenXml;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Interfaces;
namespace WebBanHang.BLL.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly ICategoryRepository _cateRepo;


        public ProductService(IProductRepository repo, ICategoryRepository cateRepo)
        {
            _repo = repo;
            _cateRepo = cateRepo;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var products = await _repo.GetAllAsync();

            return products.Select(u => new ProductDTO
            {
                ProductId = u.ProductId,
                Name = u.Name,
                Code = u.Code,
                SellingPrice = u.SellingPrice,
                Status = u.Status,
                StockQuantity = u.StockQuantity,
                Description = u.Description,
                SupplierName = u.SupplierName,
                Images = u.ProductImages.Select(x => new ProductImageDTO
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                }),
                CategoryName = u.Category.Name,
                CategoryId = u.CategoryId
            }).ToList();
        }


        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null)
                return new ProductDTO();

            return new ProductDTO
            {
                ProductId = id,
                Code = product.Code,
                Name = product.Name,
                Description = product.Description,
                Images = product.ProductImages.Select(x => new ProductImageDTO
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                }),
                Status = product.Status,
                SellingPrice = product.SellingPrice,
                ImportPrice = product.ImportPrice,
                StockQuantity = product.StockQuantity,
                SupplierName = product.SupplierName,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name
            };
        }
        public async Task<DetailProductDTO> GetDetailProductByIdAsync(int id)
        {
            var product = await _repo.GetByIdAsync(id);

            return new DetailProductDTO
            {
                ProductId = product.ProductId,
                Code = product.Code,
                Name = product.Name,
                CategoryName = product.Category.Name,
                SellingPrice = product.SellingPrice,
                ImportPrice = product.ImportPrice,
                StockQuantity = product.StockQuantity,
                SupplierName = product.SupplierName,
                Status = product.Status,
                Description = product.Description,
                CreateBy = product.CreatedByNavigation.FullName,
                CreatedDate = product.CreatedDate ?? DateTime.Now,
                ExistingImages = product.ProductImages.Select(x => new ProductImageDTO
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                }).ToList()
            };


        }



        //Admin
        public async Task<bool> CreateProductAsync(ProductDTO dto)
        {
            if (dto.CategoryId <= 0) return false;

            var existedCode = await _repo.GetByCodeIncludeDeleteAsync(dto.Code);
            if (existedCode != null) return false;

            var product = new Product
            {
                Code = dto.Code,
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                ProductImages = (dto.Images ?? Enumerable.Empty<ProductImageDTO>())
                    .Select(x => new ProductImage
                    {
                        ImageUrl = x.ImageUrl,
                        IsPrimary = x.IsPrimary ?? false
                    }).ToList(),
                SellingPrice = dto.SellingPrice,
                ImportPrice = dto.ImportPrice,
                StockQuantity = dto.StockQuantity,
                Status = dto.Status,
                SupplierName = dto.SupplierName,
                Description = dto.Description,
                CreatedDate = DateTime.Now,
                CreatedBy = dto.CreatedBy,
            };

            await _repo.CreateAsync(product);
            return true;
        }

        public async Task<bool> UpdateProductAsync(ProductDTO dto)
        {
            var existedProduct = await _repo.GetByIdIncludeDeleteAsync(dto.ProductId);
            if (existedProduct == null) return false;

            existedProduct.ProductId= dto.ProductId;
            existedProduct.Code = dto.Code;
            existedProduct.Name = dto.Name;
            existedProduct.Description = dto.Description;
            existedProduct.SellingPrice = dto.SellingPrice;
            existedProduct.ImportPrice = dto.ImportPrice;
            existedProduct.StockQuantity = dto.StockQuantity;
            existedProduct.Status = dto.Status;
            existedProduct.SupplierName = dto.SupplierName;
            existedProduct.CategoryId = dto.CategoryId;

            existedProduct.ProductImages ??= new List<ProductImage>();
            var imageDtos = dto.Images ?? new List<ProductImageDTO>();

            foreach (var existingImage in existedProduct.ProductImages)
            {
                var correspondingImage = imageDtos.FirstOrDefault(
                    x => x.ImageUrl == existingImage.ImageUrl
                );

                if (correspondingImage != null)
                {
                    existingImage.IsPrimary = correspondingImage.IsPrimary ?? false;
                }
            }

            foreach (var image in imageDtos)
            {
                var existingImage = existedProduct.ProductImages.FirstOrDefault(x => x.ImageUrl == image.ImageUrl);
                if(existingImage != null) existingImage.ImageUrl = image.ImageUrl;
                else
                {
                    existedProduct.ProductImages.Add(new ProductImage
                    {
                        ImageUrl = image.ImageUrl,
                        IsPrimary = true,
                    });
                }

            }
   

            await _repo.UpdateAsync(existedProduct);
            return true;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsIncludeDeleteAsync()
        {
            var products = await _repo.GetAllIncludeDeleteAsync();

            return products.Select(u => new ProductDTO
            {
                ProductId = u.ProductId,
                Name = u.Name,
                Code = u.Code,
                ImportPrice=u.ImportPrice,
                SellingPrice = u.SellingPrice,
                Status = u.Status,
                StockQuantity = u.StockQuantity,
                Description = u.Description,
                SupplierName = u.SupplierName,
                Images = u.ProductImages.Select(x => new ProductImageDTO
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                }),
                CategoryName = u.Category.Name,
                CategoryId = u.CategoryId
            }).ToList();
        }


        public async Task<ProductDTO> GetProductByIdIncludeDeleteAsync(int id)
        {
            var product = await _repo.GetByIdIncludeDeleteAsync(id);
            if (product == null)
                return new ProductDTO();

            return new ProductDTO
            {
                ProductId = id,
                Code = product.Code,
                Name = product.Name,
                Description = product.Description,
                Images = product.ProductImages.Select(x => new ProductImageDTO
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                }),
                Status = product.Status,
                SellingPrice = product.SellingPrice,
                ImportPrice = product.ImportPrice,
                StockQuantity = product.StockQuantity,
                SupplierName = product.SupplierName,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name
            };
        }
        public async Task<DetailProductDTO> GetDetailProductByIdIncludeDeleteAsync(int id)
        {
            var product = await _repo.GetByIdIncludeDeleteAsync(id);

            return new DetailProductDTO
            {
                ProductId = product.ProductId,
                Code = product.Code,
                Name = product.Name,
                CategoryName = product.Category.Name,
                SellingPrice = product.SellingPrice,
                ImportPrice = product.ImportPrice,
                StockQuantity = product.StockQuantity,
                SupplierName = product.SupplierName,
                Status = product.Status,
                Description = product.Description,
                CreateBy = product.CreatedByNavigation.FullName,
                CreatedDate = product.CreatedDate ?? DateTime.Now,
                ExistingImages = product.ProductImages.Select(x => new ProductImageDTO
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                }).ToList()
            };
            

        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _repo.GetByIdIncludeDeleteAsync(id);
            if (product == null) return false;

            return await _repo.DeleteAsync(id);
            
        }

        public async Task<bool> RestoreProductAsync(int id)
        {
            var product =await _repo.GetByIdIncludeDeleteAsync(id);

            if (product == null || product.Status != "Deleted")
            {
                return false;
            }

            product.Status = "Active";

            await _repo.UpdateAsync(product);

            return true;
        }
        public async Task<bool> HardDeleteProductAsync(int id)
        {
            var product = await _repo.GetByIdIncludeDeleteAsync(id);

            if (product == null) return false;

            if (product.Status != "Deleted") return false;

            var hasHistoricalReferences = await _repo.HasHistoricalReferencesAsync(id);

            if (hasHistoricalReferences) return false;

            return await _repo.HardDeleteAsync(product);
        }
        public async Task<byte[]> ExportProductsToExcelAsync()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Admin");
            var products = await GetAllProductsIncludeDeleteAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh_Sach_SanPham");

                worksheet.Cells[1, 1].Value = "Mã Sản Phẩm";
                worksheet.Cells[1, 2].Value = "Tên Sản Phẩm";
                worksheet.Cells[1, 3].Value = "Tên Danh Mục";
                worksheet.Cells[1, 4].Value = "Giá Nhập";
                worksheet.Cells[1, 5].Value = "Giá Bán";
                worksheet.Cells[1, 6].Value = "Số Lượng Tồn";
                worksheet.Cells[1, 7].Value = "Mô Tả";
                worksheet.Cells[1, 8].Value = "Trạng Thái";
                worksheet.Cells[1, 9].Value = "Nhà Cung Cấp";

                worksheet.Cells["A1:I1"].Style.Font.Bold = true;
                worksheet.Cells["A1:I1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:I1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                int row = 2;
                foreach (var item in products)
                {
                    worksheet.Cells[row, 1].Value = item.Code;
                    worksheet.Cells[row, 2].Value = item.Name;

                    worksheet.Cells[row, 3].Value = item.CategoryName;

                    worksheet.Cells[row, 4].Value = item.ImportPrice;
                    worksheet.Cells[row, 5].Value = item.SellingPrice;
                    worksheet.Cells[row, 6].Value = item.StockQuantity;
                    worksheet.Cells[row, 7].Value = item.Description;
                    worksheet.Cells[row, 8].Value = item.Status;
                    worksheet.Cells[row, 9].Value = item.SupplierName;
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                return await package.GetAsByteArrayAsync();
            }
        }

        public async Task<int> ImportProductsFromExcelAsync(Stream fileStream, int userId)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Admin");
            int importedCount = 0;

            var allCategories = await _cateRepo.GetAllAsync(); 

            using (var package = new ExcelPackage(fileStream))
            {
                if (package.Workbook.Worksheets.Count == 0)
                    throw new Exception("File Excel trống, không có dữ liệu.");

                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension?.Rows ?? 0;
                int colCount = worksheet.Dimension?.Columns ?? 0;

                if (colCount < 8)
                    throw new Exception("File Excel sai biểu mẫu. Biểu mẫu chuẩn phải có ít nhất 8 cột.");

                var col1Header = worksheet.Cells[1, 1].Value?.ToString()?.Trim().ToLower();
                var col3Header = worksheet.Cells[1, 3].Value?.ToString()?.Trim().ToLower();

                if (col1Header != "mã sản phẩm" || col3Header != "tên danh mục")
                {
                    throw new Exception("File Excel sai cấu trúc (Cột C phải là 'Tên Danh Mục'). Vui lòng Xuất Excel để lấy biểu mẫu chuẩn.");
                }

                if (rowCount < 2)
                    throw new Exception("File Excel không có dòng dữ liệu nào để nhập.");

                for (int row = 2; row <= rowCount; row++)
                {
                    var code = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var name = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name)) continue;


                    var categoryNameInExcel = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                    int mappedCategoryId = 1; 
                    if (!string.IsNullOrEmpty(categoryNameInExcel))
                    {
                        var matchedCategory = allCategories.FirstOrDefault(c =>
                            c.Name.Equals(categoryNameInExcel, StringComparison.OrdinalIgnoreCase));

                        if (matchedCategory != null)
                        {
                            mappedCategoryId = matchedCategory.CategoryId; 
                        }
                    }

                    _ = decimal.TryParse(worksheet.Cells[row, 4].Value?.ToString()?.Trim(), out decimal importPrice);
                    _ = decimal.TryParse(worksheet.Cells[row, 5].Value?.ToString()?.Trim(), out decimal sellingPrice);
                    _ = int.TryParse(worksheet.Cells[row, 6].Value?.ToString()?.Trim(), out int stockQuantity);

                    var desc = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                    var status = worksheet.Cells[row, 8].Value?.ToString()?.Trim() ?? "Active";
                    var supplierName = worksheet.Dimension?.Columns >= 9
                        ? worksheet.Cells[row, 9].Value?.ToString()?.Trim()
                        : null;
                    if (string.IsNullOrEmpty(supplierName)) supplierName = null;

                    var dto = new ProductDTO
                    {
                        Code = code,
                        Name = name,
                        CategoryId = mappedCategoryId, 
                        ImportPrice = importPrice,
                        SellingPrice = sellingPrice,
                        StockQuantity = stockQuantity,
                        Description = desc,
                        Status = status,
                        SupplierName = supplierName,
                        CreatedBy = userId,
                        Images = new List<ProductImageDTO>()
                    };

                    bool isAdded = await CreateProductAsync(dto);
                    if (isAdded) importedCount++;
                }
            }

            return importedCount;
        }
    }
}

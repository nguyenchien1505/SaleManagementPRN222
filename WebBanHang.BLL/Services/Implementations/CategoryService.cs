using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Implementations;
using WebBanHang.DAL.Repositories.Interfaces;
using OfficeOpenXml;
using System.IO;
namespace WebBanHang.BLL.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        public CategoryService(ICategoryRepository repo)
        {
            _repo = repo;
        }
        //get
        public async Task<List<CategoryDTO>> GetAllCateAsync()
        {
            var cates = await _repo.GetAllAsync();
            if (cates == null) return null;
            var dto = cates.Select(x => new CategoryDTO
            {
                CategoryId = x.CategoryId,
                Name = x.Name,
                Description = x.Description,
                ParentId = x.ParentId,
                Status = x.Status,
            }).ToList();
            return dto;
        }

        public async Task<CategoryDTO> GetCateByIdAsync(int id)
        {
            var cate = await _repo.GetByIdAsync(id);
            if (cate == null) return null;

            var dto = new CategoryDTO
            {
                CategoryId = id,
                Name = cate.Name,
                Description = cate.Description,
                ParentId = cate.ParentId,
                Status = cate.Status,
            };

            return dto;
        }
        public async Task<CategoryDTO> GetCateByNameAsync(string name)
        {
            var cate = await _repo.GetByNameAsync(name);
            if (cate == null) return null;
            var dto = new CategoryDTO
            {
                CategoryId = cate.CategoryId,
                Description = cate.Description,
                Name = cate.Name,
                Status = cate.Status,
                ParentId = cate.ParentId
            };
            return dto;
        }
        //crud
        public async Task<bool> AddCateAsync(CreateCategoryDTO dto)
        {
            if (dto == null) return false;
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                Status = dto.Status,
            };
            await _repo.AddAsync(category);
            return true;
        }

        public async Task<bool> UpdateCateAsync(UpdateCategoryDTO dto)
        {
            if (dto == null) return false;
            var category = new Category
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                Description = dto.Description,
                Status = dto.Status,
                Products = dto.Products.Select(p => new Product
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Code = p.Code,
                    Description = p.Description,
                    SellingPrice = p.SellingPrice,
                    Status = p.Status,
                    CategoryId = p.CategoryId
                }).ToList()
            };
            await _repo.UpdateAsync(category);
            return true;
        }
        public async Task<bool> DeleteCateAsync(int id)
        {
            var cate = await _repo.GetByIdAsync(id);
            if (cate == null) return false;
            await _repo.DeleteAsync(id);
            return true;
        }


        //get include deleted
        public async Task<CategoryDTO> GetCateByNameIncludeDeleteAsync(string name)
        {
            var cate = await _repo.GetByNameIncludeDeleteAsync(name);
            if (cate == null) return null;
            var dto = new CategoryDTO
            {
                CategoryId = cate.CategoryId,
                Description = cate.Description,
                Name = cate.Name,
                Status = cate.Status,
                ParentId = cate.ParentId
            };
            return dto;
        }

        public async Task<List<CategoryDTO>> GetAllCateIncludeDeleteAsync()
        {
            var cates = await _repo.GetAllIncludeDeleteAsync();
            if (cates == null) return null;
            var dto = cates.Select(x => new CategoryDTO
            {
                CategoryId = x.CategoryId,
                Name = x.Name,
                Description = x.Description,
                ParentId = x.ParentId,
                Status = x.Status,
            }).ToList();
            return dto;
        }

        public async Task<CategoryDTO> GetCateByIdIncludeDeleteAsync(int id)
        {
            var cate = await _repo.GetByIdIncludeDeleteAsync(id);
            if (cate == null) return null;

            var dto = new CategoryDTO
            {
                CategoryId = id,
                Name = cate.Name,
                Description = cate.Description,
                ParentId = cate.ParentId,
                Status = cate.Status,
            };

            return dto;
        }

        public async Task<bool> RestoreCateAsync(int categoryId)
        {
            var category = await _repo.GetByIdIncludeDeleteAsync(categoryId);

            if (category == null)
            {
                return false;
            }

            if (category.Status != "Deleted")
            {
                return false;
            }

            if (category.ParentId.HasValue)
            {
                var parent = await _repo.GetByIdIncludeDeleteAsync(category.ParentId.Value);

                if (parent == null || parent.Status == "Deleted")
                {
                    return false;
                }
            }

            category.Status = "Active";
            await _repo.UpdateAsync(category);
            return true;
        }

        public async Task<bool> HardDeleteCateAsync(int categoryId)
        {
            var category = await _repo.GetByIdIncludeDeleteAsync(categoryId);

            if (category == null)
            {
                return false;
            }

            if (category.Status != "Deleted")
            {
                return false;
            }

            var hasChildCategories =
                await _repo.HasChildCategoriesAsync(categoryId);

            if (hasChildCategories)
            {
                return false;
            }

            var hasProducts = await _repo.HasProductsAsync(categoryId);

            if (hasProducts)
            {
                return false;
            }

            try
            {
                await _repo.HardDeleteAsync(category);
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
        public async Task<byte[]> ExportCategoriesToExcelAsync()
        {
            var categories = await GetAllCateIncludeDeleteAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh_Muc_San_Pham");

                worksheet.Cells[1, 1].Value = "Mã Danh Mục";
                worksheet.Cells[1, 2].Value = "Tên Danh Mục";
                worksheet.Cells[1, 3].Value = "Mô Tả";
                worksheet.Cells[1, 4].Value = "Trạng Thái";

                worksheet.Cells["A1:D1"].Style.Font.Bold = true;
                worksheet.Cells["A1:D1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:D1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                int row = 2;
                foreach (var item in categories)
                {
                    worksheet.Cells[row, 1].Value = item.CategoryId;
                    worksheet.Cells[row, 2].Value = item.Name;
                    worksheet.Cells[row, 3].Value = item.Description;
                    worksheet.Cells[row, 4].Value = item.Status;
                    
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return await package.GetAsByteArrayAsync();
            }
        }

        public async Task<int> ImportCategoriesFromExcelAsync(Stream fileStream)
        {
            int importedCount = 0;

            using (var package = new ExcelPackage(fileStream))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    throw new Exception("File Excel trống, không có dữ liệu.");
                }

                var worksheet = package.Workbook.Worksheets[0];

                int rowCount = worksheet.Dimension?.Rows ?? 0;
                int colCount = worksheet.Dimension?.Columns ?? 0;

                if (colCount < 4)
                {
                    throw new Exception("File Excel sai biểu mẫu. Biểu mẫu chuẩn phải có ít nhất 4 cột.");
                }

                var col2Header = worksheet.Cells[1, 2].Value?.ToString()?.Trim().ToLower();
                var col3Header = worksheet.Cells[1, 3].Value?.ToString()?.Trim().ToLower();
                var col4Header = worksheet.Cells[1, 4].Value?.ToString()?.Trim().ToLower();

                if (col2Header != "tên danh mục" ||
                    col3Header != "mô tả" ||
                    col4Header != "trạng thái")
                {
                    throw new Exception("File Excel sai cấu trúc cột. Vui lòng Xuất Excel để lấy biểu mẫu chuẩn (Cột B: Tên Danh Mục, Cột C: Mô Tả, Cột D: Trạng Thái).");
                }

                if (rowCount < 2)
                {
                    throw new Exception("File Excel không có dữ liệu để nhập.");
                }

                for (int row = 2; row <= rowCount; row++)
                {
                    var name = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(name)) continue;

                    var desc = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                    var status = worksheet.Cells[row, 4].Value?.ToString()?.Trim() ?? "Active";

                    var dto = new CreateCategoryDTO
                    {
                        Name = name,
                        Description = desc,
                        Status = status
                    };

                    bool isAdded = await AddCateAsync(dto);
                    if (isAdded) importedCount++;
                }
            }

            return importedCount;
        }
        
    }
}
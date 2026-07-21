using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.BLL.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        public CategoryService(ICategoryRepository repo)
        {
            _repo = repo;
        }
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
            if(cate == null) return false;
            await _repo.DeleteAsync(id);
            return true;
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
    }
}

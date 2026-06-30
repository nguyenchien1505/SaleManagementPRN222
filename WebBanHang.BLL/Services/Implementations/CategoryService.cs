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
        public void Add(Category category)
        {
            throw new NotImplementedException();
        }

        public void Update(Category category)
        {
            throw new NotImplementedException();
        }
        public void Delete(int id)
        {
            throw new NotImplementedException();
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

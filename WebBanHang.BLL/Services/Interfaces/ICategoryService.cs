using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDTO>> GetAllCateAsync();
        Task<CategoryDTO> GetCateByIdAsync(int id);
        Task<CategoryDTO> GetCateByNameAsync(string name);
        Task<bool> AddCateAsync(CreateCategoryDTO dto);
        Task<bool> UpdateCateAsync(UpdateCategoryDTO dto);
        Task<bool> DeleteCateAsync(int id);
        //Admin
        Task<List<CategoryDTO>> GetAllCateIncludeDeleteAsync();
        Task<CategoryDTO> GetCateByIdIncludeDeleteAsync(int id);
        Task<CategoryDTO> GetCateByNameIncludeDeleteAsync(string name);
        Task<bool> RestoreCateAsync(int categoryId);
        Task<bool> HardDeleteCateAsync(int categoryId);

    }
}

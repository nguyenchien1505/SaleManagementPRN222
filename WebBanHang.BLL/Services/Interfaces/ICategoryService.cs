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
        void Add(Category category);
        void Update(Category category);
        void Delete(int id);

    }
}

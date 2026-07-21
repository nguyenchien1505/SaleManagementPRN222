
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

﻿using System.Collections.Generic;
using System.Threading.Tasks; 
using WebBanHang.DAL.Entities;

namespace WebBanHang.DAL.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task<Category> GetByNameAsync(string name);
        //Admin
        Task<IEnumerable<Category>> GetAllIncludeDeleteAsync();
        Task<Category> GetByIdIncludeDeleteAsync(int id);
        Task<Category> GetByNameIncludeDeleteAsync(string name);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<bool> HasProductsAsync(int categoryId);
        Task<bool> HasChildCategoriesAsync(int categoryId);
        Task HardDeleteAsync(Category category);
    }
}
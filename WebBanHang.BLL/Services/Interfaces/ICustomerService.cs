using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<User>> GetAllCustomersAsync(string? searchString);
        Task<User?> GetCustomerDetailsAsync(int id);
    }
}

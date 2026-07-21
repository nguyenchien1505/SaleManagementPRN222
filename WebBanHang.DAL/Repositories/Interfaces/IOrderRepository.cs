using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.DAL.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetOrdersWithCustomerAsync(string? searchString, string? statusFilter);

        Task<Order?> GetOrderDetailsAsync(int id);

        List<Order> GetAll();

        Order GetById(int id);

        void Add(Order order);

        void Update(Order order);

        void Delete(int id);
    }
}

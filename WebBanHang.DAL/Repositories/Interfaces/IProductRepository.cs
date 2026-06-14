using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.DAL.Repositories.Interfaces
{
    public interface IProductRepository
    {
        List<Product> GetAll();

        Product GetById(int id);

        List<Product> Search(string keyword);

        List<Product> GetByCategory(int categoryId);

        void Add(Product product);

        void Update(Product product);

        void Delete(int id);
    }
}

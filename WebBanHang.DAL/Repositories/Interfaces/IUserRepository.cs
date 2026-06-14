using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.DAL.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User GetByUsername(string username);
        User GetById(int id);
        List<User> GetAll();
        void Add(User user);
        void Update(User user);
        void Delete(int id);

    }
}

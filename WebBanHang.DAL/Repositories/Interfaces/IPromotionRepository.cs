using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.DAL.Repositories.Interfaces
{
    public interface IPromotionRepository
    {
        Promotion GetByCode(string code);

        List<Promotion> GetAll();

        void Add(Promotion promotion);

        void Update(Promotion promotion);

        void Delete(int id);
        void Test();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Context;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.DAL.Repositories.Implementations
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly WebBanHangContext _context;
        public PromotionRepository(WebBanHangContext context)
        {
            _context = context;
        }
        public void Add(Promotion promotion)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Promotion> GetAll()
        {
            throw new NotImplementedException();
        }

        public Promotion GetByCode(string code)
        {
            throw new NotImplementedException();
        }

        public void Update(Promotion promotion)
        {
            throw new NotImplementedException();
        }
    }
}

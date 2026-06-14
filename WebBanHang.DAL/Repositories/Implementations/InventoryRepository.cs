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
    public class InventoryRepository : IInventoryRepository
    {
        private readonly WebBanHangContext _context;
        public InventoryRepository(WebBanHangContext context)
        {
            _context = context;
        }

        public void AddTransaction(InventoryTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public List<InventoryTransaction> GetTransactionsByProduct(int productId)
        {
            throw new NotImplementedException();
        }
    }
}

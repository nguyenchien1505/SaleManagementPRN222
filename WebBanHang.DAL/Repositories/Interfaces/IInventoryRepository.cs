using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.DAL.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        void AddTransaction(InventoryTransaction transaction);

        List<InventoryTransaction> GetTransactionsByProduct(int productId);
    }
}

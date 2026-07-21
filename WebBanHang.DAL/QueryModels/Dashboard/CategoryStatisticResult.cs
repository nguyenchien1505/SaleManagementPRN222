using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.DAL.QueryModels.Dashboard
{
    public class CategoryStatisticResult
    {
        public string CategoryName { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}

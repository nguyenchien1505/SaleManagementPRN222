using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.BLL.DTOs
{
    public class CategoryStatisticDTO
    {
        public string CategoryName { get; set; } = "Unknown";

        public int Quantity { get; set; }
    }
}

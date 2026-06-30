using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.BLL.DTOs
{
    public class ProductImageDTO
    {
        public string ImageUrl { get; set; } = null!;

        public bool? IsPrimary { get; set; }
    }
}

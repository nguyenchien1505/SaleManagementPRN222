using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.DAL.Abstractions
{
    public interface ICurrentUserContext
    {
        int? UserId { get; }
        string UserName { get; }
        string? Role { get; }
        string? IpAddress { get; }
        string? RequestPath { get; }
    }
    
}

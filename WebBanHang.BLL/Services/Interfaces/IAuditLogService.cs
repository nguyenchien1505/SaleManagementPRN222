using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(string entityName, int entityId, string action, int performedBy, string? performedByName, string? description = null);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IAuditLogService
    {
        // Ghi lại 1 hành động: ai tạo/sửa sản phẩm, ai xác nhận đơn hàng, ...
        Task LogAsync(string entityName, int entityId, string action, int performedBy, string? description = null);

        Task<List<AuditLog>> GetRecentAsync(int take = 100);

        Task<List<AuditLog>> GetByEntityAsync(string entityName, int entityId);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(string entityName, int entityId, string action, int performedBy, string? performedByName, string? description = null);

        Task<(List<AuditLog> Logs, int TotalCount, List<string> Actions, List<string> EntityNames)> GetAuditLogsPagedAsync(
            string? search = null,
            string? actionName = null,
            string? entityName = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 15);
    }
}
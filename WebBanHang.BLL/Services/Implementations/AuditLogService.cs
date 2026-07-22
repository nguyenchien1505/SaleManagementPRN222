using System;
using System.Threading.Tasks;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Context;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly WebBanHangContext _context;

        public AuditLogService(WebBanHangContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string entityName, int entityId, string action, int performedBy, string? performedByName, string? description = null)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                PerformedBy = performedBy,
                PerformedByName = performedByName,
                Description = description,
                CreatedDate = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }
    }
}
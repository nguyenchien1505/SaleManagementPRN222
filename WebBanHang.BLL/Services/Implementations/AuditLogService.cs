using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task LogAsync(string entityName, int entityId, string action, int performedBy, string? description = null)
        {
            try
            {
                string? performedByName = await _context.Users
                    .Where(u => u.UserId == performedBy)
                    .Select(u => u.FullName)
                    .FirstOrDefaultAsync();

                var log = new AuditLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    PerformedBy = performedBy,
                    PerformedByName = performedByName,
                    Description = description
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Ghi log không được làm gián đoạn nghiệp vụ chính (tạo SP / xác nhận đơn ...)
            }
        }

        public async Task<List<AuditLog>> GetRecentAsync(int take = 100)
        {
            return await _context.AuditLogs
                .OrderByDescending(a => a.CreatedDate)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByEntityAsync(string entityName, int entityId)
        {
            return await _context.AuditLogs
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }
    }
}

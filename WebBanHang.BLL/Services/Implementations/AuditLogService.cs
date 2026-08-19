using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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


        public async Task<(List<AuditLog> Logs, int TotalCount, List<string> Actions, List<string> EntityNames)> GetAuditLogsPagedAsync(
            string? search = null,
            string? actionName = null,
            string? entityName = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 15)
        {
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            var distinctActions = await query.Select(x => x.Action).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToListAsync();
            var distinctEntities = await query.Select(x => x.EntityName).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToListAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    (x.PerformedByName != null && x.PerformedByName.Contains(search)) ||
                    (x.Description != null && x.Description.Contains(search)) ||
                    (x.EntityName != null && x.EntityName.Contains(search)) ||
                    (x.Action != null && x.Action.Contains(search)) ||
                    x.EntityId.ToString().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(actionName))
            {
                query = query.Where(x => x.Action == actionName);
            }

            if (!string.IsNullOrWhiteSpace(entityName))
            {
                query = query.Where(x => x.EntityName == entityName);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreatedDate <= endOfDay);
            }

            int totalCount = await query.CountAsync();

            int skip = (page - 1) * pageSize;
            var logs = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount, distinctActions, distinctEntities);
        }
    }
}
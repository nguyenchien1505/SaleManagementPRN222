using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebBanHang.DAL.Abstractions;
using WebBanHang.DAL.Entities;

namespace WebBanHang.DAL.Interceptors
{
    public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserContext _currentUser;

        private static readonly HashSet<string> SensitiveProperties =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Password",
                "PasswordHash",
                "ResetPasswordToken",
                "ClientSecret",
                "Token"
            };

        private readonly List<(AuditLog AuditLog, EntityEntry Entry)> _pendingAuditEntries = new();

        public AuditSaveChangesInterceptor(ICurrentUserContext currentUser)
        {
            _currentUser = currentUser;
        }

        public override InterceptionResult<int> SavingChanges( DbContextEventData eventData,
                                                               InterceptionResult<int> result)
        {
            PrepareAuditLogs(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            PrepareAuditLogs(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            SavePendingAuditLogs(eventData.Context);
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData,
                                                                int result,
                                                                CancellationToken cancellationToken = default)
        {
            await SavePendingAuditLogsAsync(eventData.Context, cancellationToken);
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void PrepareAuditLogs(DbContext? context)
        {
            _pendingAuditEntries.Clear();

            if (context is null)
            {
                return;
            }

            context.ChangeTracker.DetectChanges();

            var entries = context.ChangeTracker.Entries().Where(ShouldAudit).ToList();

            foreach (var entry in entries)
            {
                var auditLog = CreateAuditLog(entry);
                _pendingAuditEntries.Add((auditLog, entry));
            }
        }

        private void SavePendingAuditLogs(DbContext? context)
        {
            if (context is null || _pendingAuditEntries.Count == 0)
            {
                return;
            }

            var entriesToSave = _pendingAuditEntries.ToList();
            _pendingAuditEntries.Clear();

            var logsToAdd = new List<AuditLog>();

            foreach (var (auditLog, entry) in entriesToSave)
            {
                if (auditLog.EntityId == 0)
                {
                    auditLog.EntityId = GetPrimaryKey(entry) ?? 0;
                }
                logsToAdd.Add(auditLog);
            }

            if (logsToAdd.Count > 0)
            {
                context.Set<AuditLog>().AddRange(logsToAdd);
                context.SaveChanges();
            }
        }

        private async Task SavePendingAuditLogsAsync(DbContext? context, CancellationToken cancellationToken)
        {
            if (context is null || _pendingAuditEntries.Count == 0)
            {
                return;
            }

            var entriesToSave = _pendingAuditEntries.ToList();

            _pendingAuditEntries.Clear();

            var logsToAdd = new List<AuditLog>();

            foreach (var (auditLog, entry) in entriesToSave)
            {
                if (auditLog.EntityId == 0)
                {
                    auditLog.EntityId = GetPrimaryKey(entry) ?? 0;
                }
                logsToAdd.Add(auditLog);
            }

            if (logsToAdd.Count > 0)
            {
                context.Set<AuditLog>().AddRange(logsToAdd);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private static bool ShouldAudit(EntityEntry entry)
        {
            if (entry.Entity is AuditLog)
            {
                return false;
            }

            return entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted;
        }

        private AuditLog CreateAuditLog(EntityEntry entry)
        {
            return new AuditLog
            {
                Action = GetAction(entry.State),
                EntityName = entry.Metadata.ClrType.Name,
                EntityId = GetPrimaryKey(entry) ?? 0,
                PerformedBy = _currentUser.UserId ?? 0,
                PerformedByName = _currentUser.UserName,
                Description = BuildDescription(entry),
                CreatedDate = DateTime.Now
            };
        }

        private static string GetAction(EntityState state)
        {
            return state switch
            {
                EntityState.Added => "CREATE",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => "UNKNOWN"
            };
        }

        private static int? GetPrimaryKey(EntityEntry entry)
        {
            var primaryKey = entry.Metadata.FindPrimaryKey();
            if (primaryKey == null || primaryKey.Properties.Count == 0)
            {
                return null;
            }

            var pkProperty = primaryKey.Properties[0];
            var value = entry.Property(pkProperty.Name).CurrentValue;

            if (value != null && int.TryParse(value.ToString(), out int id) && id > 0)
            {
                return id;
            }

            return null;
        }

        private static string BuildDescription(EntityEntry entry)
        {
            var action = GetAction(entry.State);
            var entityName = entry.Metadata.ClrType.Name;

            return $"{action} entity {entityName}";
        }
    }
}

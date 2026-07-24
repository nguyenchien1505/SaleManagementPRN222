using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.Filters;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RoleAuthorize("Admin")]
    public class AuditLogController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public async Task<IActionResult> Index(string? search,
                                              string? actionName,
                                              string? entityName,
                                              DateTime? fromDate,
                                              DateTime? toDate,
                                              int page = 1)
        {
            int pageSize = 15;
            if (page < 1) page = 1;

            var (logs, totalCount, actions, entityNames) = await _auditLogService.GetAuditLogsPagedAsync(
                search, actionName, entityName, fromDate, toDate, page, pageSize);

            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (totalPages < 1) totalPages = 1;

            ViewBag.Search = search;
            ViewBag.ActionName = actionName;
            ViewBag.EntityName = entityName;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.Actions = actions;
            ViewBag.EntityNames = entityNames;

            return View(logs);
        }
    }
}
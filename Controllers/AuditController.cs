using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FleetManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Audit/Index
        public async Task<IActionResult> Index(string? entityType, string? action, string? userName,
            DateTime? fromDate, DateTime? toDate, string? status)
        {
            var query = _context.AuditLogs.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);

            if (!string.IsNullOrEmpty(userName))
                query = query.Where(a => a.UserName != null && a.UserName.Contains(userName));

            if (fromDate.HasValue)
                query = query.Where(a => a.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Timestamp <= toDate.Value.AddDays(1));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Take(1000) // Limit for performance
                .ToListAsync();

            // Statistics
            ViewBag.TotalLogs = await _context.AuditLogs.CountAsync();
            ViewBag.TodayLogs = await _context.AuditLogs.CountAsync(a => a.Timestamp.Date == DateTime.UtcNow.Date);
            ViewBag.FailedActions = await _context.AuditLogs.CountAsync(a => a.Status == "Failed");
            ViewBag.UniqueUsers = await _context.AuditLogs.Select(a => a.UserId).Distinct().CountAsync();

            // Filter options
            ViewBag.EntityTypes = await _context.AuditLogs.Select(a => a.EntityType).Distinct().ToListAsync();
            ViewBag.Actions = await _context.AuditLogs.Select(a => a.Action).Distinct().ToListAsync();

            // Current filters
            ViewBag.CurrentEntityType = entityType;
            ViewBag.CurrentAction = action;
            ViewBag.CurrentUserName = userName;
            ViewBag.CurrentFromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentStatus = status;

            return View(logs);
        }

        // GET: Audit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var log = await _context.AuditLogs.FindAsync(id);
            if (log == null) return NotFound();

            return View(log);
        }

        // GET: Audit/SecurityEvents
        public async Task<IActionResult> SecurityEvents(string? eventType, string? severity,
            DateTime? fromDate, DateTime? toDate, bool? unresolvedOnly)
        {
            var query = _context.SecurityEvents.AsQueryable();

            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(e => e.EventType == eventType);

            if (!string.IsNullOrEmpty(severity))
                query = query.Where(e => e.Severity == severity);

            if (fromDate.HasValue)
                query = query.Where(e => e.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(e => e.Timestamp <= toDate.Value.AddDays(1));

            if (unresolvedOnly == true)
                query = query.Where(e => !e.IsResolved);

            var events = await query
                .OrderByDescending(e => e.Timestamp)
                .Take(1000)
                .ToListAsync();

            // Statistics
            ViewBag.TotalEvents = await _context.SecurityEvents.CountAsync();
            ViewBag.CriticalEvents = await _context.SecurityEvents.CountAsync(e => e.Severity == "Critical");
            ViewBag.UnresolvedEvents = await _context.SecurityEvents.CountAsync(e => !e.IsResolved);
            ViewBag.TodayEvents = await _context.SecurityEvents.CountAsync(e => e.Timestamp.Date == DateTime.UtcNow.Date);

            ViewBag.EventTypes = await _context.SecurityEvents.Select(e => e.EventType).Distinct().ToListAsync();

            return View(events);
        }

        // POST: Audit/ResolveSecurityEvent/5
        [HttpPost]
        public async Task<IActionResult> ResolveSecurityEvent(int id, string resolution)
        {
            var securityEvent = await _context.SecurityEvents.FindAsync(id);
            if (securityEvent == null) return NotFound();

            securityEvent.IsResolved = true;
            securityEvent.ResolvedBy = User.Identity?.Name;
            securityEvent.ResolvedAt = DateTime.UtcNow;
            securityEvent.Resolution = resolution;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Security event resolved.";

            return RedirectToAction(nameof(SecurityEvents));
        }

        // GET: Audit/DataAccess
        public async Task<IActionResult> DataAccess(string? userName, string? entityType,
            DateTime? fromDate, DateTime? toDate, bool? piiOnly)
        {
            var query = _context.DataAccessLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userName))
                query = query.Where(d => d.UserName != null && d.UserName.Contains(userName));

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(d => d.EntityType == entityType);

            if (fromDate.HasValue)
                query = query.Where(d => d.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(d => d.Timestamp <= toDate.Value.AddDays(1));

            if (piiOnly == true)
                query = query.Where(d => d.ContainsPii);

            var logs = await query
                .OrderByDescending(d => d.Timestamp)
                .Take(1000)
                .ToListAsync();

            ViewBag.TotalAccesses = await _context.DataAccessLogs.CountAsync();
            ViewBag.PiiAccesses = await _context.DataAccessLogs.CountAsync(d => d.ContainsPii);
            ViewBag.UniqueUsers = await _context.DataAccessLogs.Select(d => d.UserId).Distinct().CountAsync();

            return View(logs);
        }

        // GET: Audit/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var now = DateTime.UtcNow;
            var last24Hours = now.AddHours(-24);
            var last7Days = now.AddDays(-7);
            var last30Days = now.AddDays(-30);

            // Activity stats
            ViewBag.Actions24h = await _context.AuditLogs.CountAsync(a => a.Timestamp >= last24Hours);
            ViewBag.Actions7d = await _context.AuditLogs.CountAsync(a => a.Timestamp >= last7Days);
            ViewBag.Actions30d = await _context.AuditLogs.CountAsync(a => a.Timestamp >= last30Days);

            // Security stats
            ViewBag.SecurityEvents24h = await _context.SecurityEvents.CountAsync(e => e.Timestamp >= last24Hours);
            ViewBag.FailedLogins24h = await _context.SecurityEvents
                .CountAsync(e => e.Timestamp >= last24Hours && e.EventType == "LoginFailure");
            ViewBag.CriticalEvents = await _context.SecurityEvents.CountAsync(e => e.Severity == "Critical" && !e.IsResolved);

            // Top users by activity
            ViewBag.TopUsers = await _context.AuditLogs
                .Where(a => a.Timestamp >= last7Days)
                .GroupBy(a => new { a.UserId, a.UserName })
                .Select(g => new
                {
                    g.Key.UserId,
                    g.Key.UserName,
                    ActionCount = g.Count()
                })
                .OrderByDescending(x => x.ActionCount)
                .Take(10)
                .ToListAsync();

            // Actions by type (last 7 days)
            ViewBag.ActionsByType = await _context.AuditLogs
                .Where(a => a.Timestamp >= last7Days)
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // Entity types accessed (last 7 days)
            ViewBag.EntityTypesAccessed = await _context.AuditLogs
                .Where(a => a.Timestamp >= last7Days)
                .GroupBy(a => a.EntityType)
                .Select(g => new { EntityType = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // Recent security events
            ViewBag.RecentSecurityEvents = await _context.SecurityEvents
                .OrderByDescending(e => e.Timestamp)
                .Take(10)
                .ToListAsync();

            // Suspicious activity (multiple failed logins)
            ViewBag.SuspiciousActivity = await _context.SecurityEvents
                .Where(e => e.EventType == "LoginFailure" && e.FailedAttemptCount >= 3)
                .OrderByDescending(e => e.Timestamp)
                .Take(5)
                .ToListAsync();

            return View();
        }

        // GET: Audit/UserActivity/5
        public async Task<IActionResult> UserActivity(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");

            var userLogs = await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .Take(500)
                .ToListAsync();

            var userSecurityEvents = await _context.SecurityEvents
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Timestamp)
                .Take(100)
                .ToListAsync();

            ViewBag.UserId = userId;
            ViewBag.UserName = userLogs.FirstOrDefault()?.UserName ?? userId;
            ViewBag.SecurityEvents = userSecurityEvents;

            // User activity summary
            ViewBag.TotalActions = await _context.AuditLogs.CountAsync(a => a.UserId == userId);
            ViewBag.LastActive = await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .MaxAsync(a => (DateTime?)a.Timestamp);

            ViewBag.ActionsByEntity = await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .GroupBy(a => a.EntityType)
                .Select(g => new { EntityType = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return View(userLogs);
        }

        // GET: Audit/Export
        public async Task<IActionResult> Export(string? entityType, string? action, string? userName,
            DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);

            if (!string.IsNullOrEmpty(userName))
                query = query.Where(a => a.UserName != null && a.UserName.Contains(userName));

            if (fromDate.HasValue)
                query = query.Where(a => a.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Timestamp <= toDate.Value.AddDays(1));

            var logs = await query.OrderByDescending(a => a.Timestamp).ToListAsync();

            // Create CSV
            var csv = new StringBuilder();
            csv.AppendLine("Timestamp,User,Role,Action,EntityType,EntityId,EntityName,Status,IPAddress,ChangedFields,Notes");

            foreach (var log in logs)
            {
                csv.AppendLine($"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{log.UserName}\",\"{log.UserRole}\",\"{log.Action}\",\"{log.EntityType}\",\"{log.EntityId}\",\"{log.EntityName}\",\"{log.Status}\",\"{log.IpAddress}\",\"{log.ChangedFields}\",\"{log.Notes}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"AuditLog_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        // GET: Audit/PurgeOldLogs
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PurgeOldLogs(int daysToKeep = 365)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            // Archive first (in production, you'd move to archive table)
            var oldLogs = await _context.AuditLogs
                .Where(a => a.Timestamp < cutoffDate && !a.IsArchived)
                .ToListAsync();

            foreach (var log in oldLogs)
            {
                log.IsArchived = true;
                log.ArchivedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // For now, we just mark as archived. In production, you might delete or move to cold storage
            TempData["Success"] = $"Archived {oldLogs.Count} old audit logs (older than {daysToKeep} days).";

            return RedirectToAction(nameof(Index));
        }
    }
}

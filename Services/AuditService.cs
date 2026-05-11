using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace FleetManagementSystem.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(string action, string entityType, string? entityId, string? entityName,
            object? oldValues = null, object? newValues = null, string? notes = null);

        Task LogViewAsync(string entityType, string? entityId, string? entityName, int recordsCount = 1);

        Task LogExportAsync(string entityType, string exportFormat, int recordsCount, string? reason = null);

        Task LogSecurityEventAsync(string eventType, string severity, string description, string? details = null);

        Task LogDataAccessAsync(string entityType, string? entityId, string accessType, string? purpose = null,
            int recordsCount = 1, bool containsPii = false, string? fields = null);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogActionAsync(string action, string entityType, string? entityId, string? entityName,
            object? oldValues = null, object? newValues = null, string? notes = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var user = httpContext?.User;

                var auditLog = new AuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityName = entityName,
                    UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System",
                    UserName = user?.Identity?.Name ?? "System",
                    UserRole = GetUserRole(user),
                    IpAddress = GetClientIpAddress(httpContext),
                    UserAgent = httpContext?.Request.Headers["User-Agent"].ToString(),
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                    ChangedFields = GetChangedFields(oldValues, newValues),
                    HttpMethod = httpContext?.Request.Method,
                    RequestPath = httpContext?.Request.Path,
                    QueryString = httpContext?.Request.QueryString.ToString(),
                    Status = "Success",
                    SessionId = httpContext?.Session?.Id,
                    Notes = notes,
                    ContainsSensitiveData = ContainsSensitiveData(entityType),
                    DataClassification = GetDataClassification(entityType),
                    RetentionExpiryDate = DateTime.UtcNow.AddYears(7) // 7 year retention
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log");
            }
        }

        public async Task LogViewAsync(string entityType, string? entityId, string? entityName, int recordsCount = 1)
        {
            await LogActionAsync("View", entityType, entityId, entityName, null, null,
                $"Viewed {recordsCount} record(s)");
        }

        public async Task LogExportAsync(string entityType, string exportFormat, int recordsCount, string? reason = null)
        {
            var notes = $"Exported {recordsCount} records to {exportFormat}";
            if (!string.IsNullOrEmpty(reason))
                notes += $". Reason: {reason}";

            await LogActionAsync("Export", entityType, null, null, null, null, notes);
        }

        public async Task LogSecurityEventAsync(string eventType, string severity, string description, string? details = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var user = httpContext?.User;

                var securityEvent = new SecurityEvent
                {
                    Timestamp = DateTime.UtcNow,
                    EventType = eventType,
                    UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier),
                    UserName = user?.Identity?.Name,
                    IpAddress = GetClientIpAddress(httpContext),
                    UserAgent = httpContext?.Request.Headers["User-Agent"].ToString(),
                    Severity = severity,
                    Description = description,
                    Details = details,
                    SessionId = httpContext?.Session?.Id
                };

                // Check for multiple failed logins
                if (eventType == "LoginFailure" && securityEvent.UserId != null)
                {
                    var recentFailures = await _context.SecurityEvents
                        .Where(e => e.UserId == securityEvent.UserId &&
                                    e.EventType == "LoginFailure" &&
                                    e.Timestamp >= DateTime.UtcNow.AddMinutes(-30))
                        .CountAsync();

                    securityEvent.FailedAttemptCount = recentFailures + 1;
                    securityEvent.IsAccountLocked = recentFailures >= 4; // Lock after 5 attempts
                }

                _context.SecurityEvents.Add(securityEvent);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security event");
            }
        }

        public async Task LogDataAccessAsync(string entityType, string? entityId, string accessType,
            string? purpose = null, int recordsCount = 1, bool containsPii = false, string? fields = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var user = httpContext?.User;

                var dataAccess = new DataAccessLog
                {
                    Timestamp = DateTime.UtcNow,
                    UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System",
                    UserName = user?.Identity?.Name ?? "System",
                    EntityType = entityType,
                    EntityId = entityId,
                    AccessType = accessType,
                    IpAddress = GetClientIpAddress(httpContext),
                    Purpose = purpose,
                    RecordsAccessed = recordsCount,
                    ContainsPii = containsPii,
                    ContainsFinancialData = entityType == "Expense" || entityType == "Budget",
                    DataFieldsAccessed = fields,
                    IsAuthorized = true
                };

                _context.DataAccessLogs.Add(dataAccess);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log data access");
            }
        }

        private string GetClientIpAddress(HttpContext? httpContext)
        {
            if (httpContext == null) return "Unknown";

            // Check for forwarded headers (when behind proxy)
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private string GetUserRole(ClaimsPrincipal? user)
        {
            if (user == null) return "Anonymous";

            if (user.IsInRole("Admin")) return "Admin";
            if (user.IsInRole("Manager")) return "Manager";
            if (user.IsInRole("Driver")) return "Driver";

            return "User";
        }

        private string? GetChangedFields(object? oldValues, object? newValues)
        {
            if (oldValues == null || newValues == null) return null;

            try
            {
                var oldDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(oldValues));
                var newDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(newValues));

                if (oldDict == null || newDict == null) return null;

                var changedFields = new List<string>();
                foreach (var key in newDict.Keys)
                {
                    if (!oldDict.ContainsKey(key) || oldDict[key].ToString() != newDict[key].ToString())
                    {
                        changedFields.Add(key);
                    }
                }

                return changedFields.Count > 0 ? string.Join(",", changedFields) : null;
            }
            catch
            {
                return null;
            }
        }

        private bool ContainsSensitiveData(string entityType)
        {
            var sensitiveTypes = new[] { "Driver", "Expense", "Document", "FuelRecord" };
            return sensitiveTypes.Contains(entityType);
        }

        private string GetDataClassification(string entityType)
        {
            return entityType switch
            {
                "Driver" => "Confidential",
                "Expense" => "Confidential",
                "Document" => "Internal",
                "FuelRecord" => "Internal",
                _ => "Public"
            };
        }
    }
}

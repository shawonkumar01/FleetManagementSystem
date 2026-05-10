using FleetManagementSystem.Data;
using FleetManagementSystem.Hubs;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Services
{
    public class AlertService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<DashboardHub> _hubContext;

        public AlertService(ApplicationDbContext context, IHubContext<DashboardHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Check for driver license expirations (within 30 days)
        public async Task CheckLicenseExpirations()
        {
            var expirationThreshold = DateTime.UtcNow.AddDays(30);
            var expiringLicenses = await _context.Drivers
                .Where(d => d.LicenseExpiry <= expirationThreshold && d.Status == "Active")
                .ToListAsync();

            foreach (var driver in expiringLicenses)
            {
                var daysUntilExpiry = (driver.LicenseExpiry - DateTime.UtcNow).Days;
                var alertType = daysUntilExpiry <= 7 ? "Danger" : "Warning";
                var message = daysUntilExpiry <= 0 
                    ? $"Driver {driver.FirstName} {driver.LastName}'s license has EXPIRED!"
                    : $"Driver {driver.FirstName} {driver.LastName}'s license expires in {daysUntilExpiry} days.";

                // Check if alert already exists
                var existingAlert = await _context.Alerts
                    .FirstOrDefaultAsync(a => a.Category == "License" && a.RelatedId == driver.Id && !a.IsRead);

                if (existingAlert == null)
                {
                    var alert = new Alert
                    {
                        Title = "License Expiration Alert",
                        Message = message,
                        Type = alertType,
                        Category = "License",
                        RelatedId = driver.Id,
                        RelatedType = "Driver",
                        ExpiresAt = driver.LicenseExpiry
                    };

                    _context.Alerts.Add(alert);

                    // Broadcast real-time notification
                    await DashboardHub.BroadcastMaintenanceAlert(_hubContext,
                        $"{driver.FirstName} {driver.LastName}",
                        daysUntilExpiry <= 0 ? "License Expired" : "License Expiring Soon",
                        driver.LicenseExpiry);
                }
            }

            await _context.SaveChangesAsync();
        }

        // Check for upcoming maintenance (within 7 days)
        public async Task CheckUpcomingMaintenance()
        {
            var maintenanceThreshold = DateTime.UtcNow.AddDays(7);
            var upcomingMaintenance = await _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .Where(m => m.NextServiceDate.HasValue && 
                            m.NextServiceDate <= maintenanceThreshold && 
                            m.Status != "Completed")
                .ToListAsync();

            foreach (var maint in upcomingMaintenance)
            {
                var daysUntil = (maint.NextServiceDate!.Value - DateTime.UtcNow).Days;
                var alertType = daysUntil <= 3 ? "Danger" : "Warning";

                // Check if alert already exists
                var existingAlert = await _context.Alerts
                    .FirstOrDefaultAsync(a => a.Category == "Maintenance" && a.RelatedId == maint.Id && !a.IsRead);

                if (existingAlert == null)
                {
                    var alert = new Alert
                    {
                        Title = "Maintenance Due Alert",
                        Message = $"Vehicle {maint.Vehicle.Make} {maint.Vehicle.Model} needs {maint.ServiceType} in {daysUntil} days.",
                        Type = alertType,
                        Category = "Maintenance",
                        RelatedId = maint.Id,
                        RelatedType = "Maintenance",
                        ExpiresAt = maint.NextServiceDate
                    };

                    _context.Alerts.Add(alert);

                    await DashboardHub.BroadcastMaintenanceAlert(_hubContext,
                        $"{maint.Vehicle.Make} {maint.Vehicle.Model}",
                        maint.ServiceType,
                        maint.NextServiceDate.Value);
                }
            }

            await _context.SaveChangesAsync();
        }

        // Get all unread alerts
        public async Task<List<Alert>> GetUnreadAlerts(int limit = 10)
        {
            return await _context.Alerts
                .Where(a => !a.IsRead && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow))
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        // Get alerts count by type
        public async Task<int> GetUnreadCount()
        {
            return await _context.Alerts
                .CountAsync(a => !a.IsRead && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow));
        }

        // Mark alert as read
        public async Task MarkAsRead(int alertId)
        {
            var alert = await _context.Alerts.FindAsync(alertId);
            if (alert != null)
            {
                alert.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        // Run all checks
        public async Task RunAllChecks()
        {
            await CheckLicenseExpirations();
            await CheckUpcomingMaintenance();
        }
    }
}

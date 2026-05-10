using FleetManagementSystem.Data;
using FleetManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Controllers
{
    [Authorize]
    public class AlertsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AlertService _alertService;

        public AlertsController(ApplicationDbContext context, AlertService alertService)
        {
            _context = context;
            _alertService = alertService;
        }

        public async Task<IActionResult> Index()
        {
            var alerts = await _context.Alerts
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return View(alerts);
        }

        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _alertService.GetUnreadCount();
            return Json(new { count });
        }

        public async Task<IActionResult> GetUnreadAlerts()
        {
            var alerts = await _alertService.GetUnreadAlerts(5);
            return PartialView("_AlertDropdown", alerts);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _alertService.MarkAsRead(id);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var unreadAlerts = await _context.Alerts
                .Where(a => !a.IsRead)
                .ToListAsync();
            
            foreach (var alert in unreadAlerts)
            {
                alert.IsRead = true;
            }
            
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // Admin action: Run all alert checks manually
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> RunChecks()
        {
            await _alertService.RunAllChecks();
            return Json(new { success = true, message = "Alert checks completed" });
        }
    }
}

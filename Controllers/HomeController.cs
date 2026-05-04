using FleetManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";

            ViewBag.TotalVehicles = await _context.Vehicles.CountAsync();
            ViewBag.ActiveVehicles = await _context.Vehicles.CountAsync(v => v.Status == "Active");
            ViewBag.TotalDrivers = await _context.Drivers.CountAsync();
            ViewBag.ActiveDrivers = await _context.Drivers.CountAsync(d => d.Status == "Active");
            ViewBag.TotalTrips = await _context.Trips.CountAsync();
            ViewBag.OngoingTrips = await _context.Trips.CountAsync(t => t.Status == "InProgress");
            ViewBag.PendingMaint = await _context.MaintenanceRecords.CountAsync(m => m.Status == "Scheduled");

            ViewBag.RecentTrips = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .OrderByDescending(t => t.StartTime)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentMaintenance = await _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .OrderByDescending(m => m.ServiceDate)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
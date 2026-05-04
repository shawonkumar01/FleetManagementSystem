using FleetManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Reports";

            // Fleet Overview
            ViewBag.TotalVehicles = await _context.Vehicles.CountAsync();
            ViewBag.ActiveVehicles = await _context.Vehicles.CountAsync(v => v.Status == "Active");
            ViewBag.InMaintenanceVehicles = await _context.Vehicles.CountAsync(v => v.Status == "InMaintenance");
            ViewBag.RetiredVehicles = await _context.Vehicles.CountAsync(v => v.Status == "Retired");

            // Driver Stats
            ViewBag.TotalDrivers = await _context.Drivers.CountAsync();
            ViewBag.ActiveDrivers = await _context.Drivers.CountAsync(d => d.Status == "Active");
            ViewBag.ExpiredLicenses = await _context.Drivers.CountAsync(d => d.LicenseExpiry < DateTime.Now);

            // Trip Stats
            ViewBag.TotalTrips = await _context.Trips.CountAsync();
            ViewBag.CompletedTrips = await _context.Trips.CountAsync(t => t.Status == "Completed");
            ViewBag.PlannedTrips = await _context.Trips.CountAsync(t => t.Status == "Planned");
            ViewBag.TotalDistance = await _context.Trips.SumAsync(t => t.DistanceKm);

            // Maintenance Stats
            ViewBag.TotalMaintenance = await _context.MaintenanceRecords.CountAsync();
            ViewBag.ScheduledMaint = await _context.MaintenanceRecords.CountAsync(m => m.Status == "Scheduled");
            ViewBag.TotalMaintenanceCost = await _context.MaintenanceRecords.SumAsync(m => m.Cost);

            // Fuel Stats
            ViewBag.TotalFuelRecords = await _context.FuelRecords.CountAsync();
            ViewBag.TotalFuelLiters = await _context.FuelRecords.SumAsync(f => f.LitersFilled);
            ViewBag.TotalFuelCost = await _context.FuelRecords.SumAsync(f => f.TotalCost);

            // Fuel per Vehicle
            ViewBag.FuelReport = await _context.FuelRecords
                .Include(f => f.Vehicle)
                .GroupBy(f => new { f.VehicleId, f.Vehicle.Make, f.Vehicle.Model, f.Vehicle.LicensePlate })
                .Select(g => new
                {
                    g.Key.Make,
                    g.Key.Model,
                    g.Key.LicensePlate,
                    TotalRecords = g.Count(),
                    TotalLiters = g.Sum(f => f.LitersFilled),
                    TotalCost = g.Sum(f => f.TotalCost),
                    AvgPerLiter = g.Average(f => (double)f.PricePerLiter)
                })
                .ToListAsync();

            // Vehicle Report
            ViewBag.VehicleReport = await _context.Vehicles
                .Select(v => new
                {
                    v.Id,
                    v.Make,
                    v.Model,
                    v.LicensePlate,
                    v.Status,
                    v.Mileage,
                    TripCount = v.Trips.Count,
                    TotalDistance = v.Trips.Sum(t => t.DistanceKm),
                    MaintCount = v.MaintenanceRecords.Count,
                    TotalMaintCost = v.MaintenanceRecords.Sum(m => m.Cost)
                })
                .ToListAsync();

            // Driver Report
            ViewBag.DriverReport = await _context.Drivers
                .Select(d => new
                {
                    d.Id,
                    d.FirstName,
                    d.LastName,
                    d.LicenseNumber,
                    d.Status,
                    d.LicenseExpiry,
                    TripCount = d.Trips.Count,
                    TotalDistance = d.Trips.Sum(t => t.DistanceKm),
                    CompletedTrips = d.Trips.Count(t => t.Status == "Completed")
                })
                .ToListAsync();

            return View();
        }
    }
}
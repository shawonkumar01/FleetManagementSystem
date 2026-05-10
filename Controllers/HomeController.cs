using FleetManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Controllers
{
    [Authorize]
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
            ViewBag.TotalFuelCost = await _context.FuelRecords.SumAsync(f => f.TotalCost);
            ViewBag.TotalFuelLiters = await _context.FuelRecords.SumAsync(f => f.LitersFilled);

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

            // Chart Data: Fuel Consumption by Month
            var fuelRecords = await _context.FuelRecords
                .OrderBy(f => f.FuelDate)
                .ToListAsync();
            var fuelByMonth = fuelRecords
                .GroupBy(f => f.FuelDate.ToString("MMM yyyy"))
                .Select(g => new { Month = g.Key, Total = g.Sum(f => f.LitersFilled) })
                .TakeLast(6)
                .ToList();
            ViewBag.FuelChartLabels = fuelByMonth.Select(f => f.Month).ToList();
            ViewBag.FuelChartData = fuelByMonth.Select(f => f.Total).ToList();

            // Chart Data: Trip Status Distribution
            var tripStatusCounts = await _context.Trips
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            ViewBag.TripStatusData = new List<int>
            {
                tripStatusCounts.FirstOrDefault(s => s.Status == "Completed")?.Count ?? 0,
                tripStatusCounts.FirstOrDefault(s => s.Status == "InProgress")?.Count ?? 0,
                tripStatusCounts.FirstOrDefault(s => s.Status == "Planned")?.Count ?? 0,
                tripStatusCounts.FirstOrDefault(s => s.Status == "Cancelled")?.Count ?? 0
            };

            // Chart Data: Monthly Cost Analysis
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .OrderBy(d => d)
                .Select(d => d.ToString("MMM yyyy"))
                .ToList();
            ViewBag.CostChartLabels = last6Months;

            var fuelCostsByMonth = fuelRecords
                .GroupBy(f => f.FuelDate.ToString("MMM yyyy"))
                .ToDictionary(g => g.Key, g => g.Sum(f => f.TotalCost));
            ViewBag.FuelCostData = last6Months.Select(m => fuelCostsByMonth.ContainsKey(m) ? (double)fuelCostsByMonth[m] : 0).ToList();

            var maintRecords = await _context.MaintenanceRecords.ToListAsync();
            var maintCostsByMonth = maintRecords
                .GroupBy(m => m.ServiceDate.ToString("MMM yyyy"))
                .ToDictionary(g => g.Key, g => g.Sum(m => m.Cost));
            ViewBag.MaintenanceCostData = last6Months.Select(m => maintCostsByMonth.ContainsKey(m) ? (double)maintCostsByMonth[m] : 0).ToList();

            return View();
        }
    }
}
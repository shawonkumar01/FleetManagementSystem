using FleetManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DashboardApi/summary
        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetSummary()
        {
            var summary = new
            {
                Vehicles = new
                {
                    Total = await _context.Vehicles.CountAsync(),
                    Active = await _context.Vehicles.CountAsync(v => v.Status == "Active"),
                    InMaintenance = await _context.Vehicles.CountAsync(v => v.Status == "InMaintenance")
                },
                Drivers = new
                {
                    Total = await _context.Drivers.CountAsync(),
                    Active = await _context.Drivers.CountAsync(d => d.Status == "Active")
                },
                Trips = new
                {
                    Total = await _context.Trips.CountAsync(),
                    Today = await _context.Trips.CountAsync(t => t.StartTime.Date == DateTime.UtcNow.Date),
                    Ongoing = await _context.Trips.CountAsync(t => t.Status == "InProgress"),
                    Completed = await _context.Trips.CountAsync(t => t.Status == "Completed")
                },
                Maintenance = new
                {
                    Scheduled = await _context.MaintenanceRecords.CountAsync(m => m.Status == "Scheduled"),
                    Overdue = await _context.MaintenanceRecords.CountAsync(m => m.Status == "Overdue")
                },
                Fuel = new
                {
                    TotalCost = await _context.FuelRecords.SumAsync(f => f.TotalCost),
                    TotalLiters = await _context.FuelRecords.SumAsync(f => f.LitersFilled),
                    ThisMonth = await _context.FuelRecords
                        .Where(f => f.FuelDate.Month == DateTime.UtcNow.Month && f.FuelDate.Year == DateTime.UtcNow.Year)
                        .SumAsync(f => f.TotalCost)
                }
            };

            return Ok(summary);
        }

        // GET: api/DashboardApi/recent
        [HttpGet("recent")]
        public async Task<ActionResult<object>> GetRecentActivity([FromQuery] int limit = 5)
        {
            var recentTrips = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .OrderByDescending(t => t.StartTime)
                .Take(limit)
                .Select(t => new {
                    Type = "Trip",
                    t.Id,
                    Description = $"{t.Vehicle.Make} {t.Vehicle.Model}: {t.Origin} to {t.Destination}",
                    t.Status,
                    Date = t.StartTime
                })
                .ToListAsync();

            var recentFuel = await _context.FuelRecords
                .Include(f => f.Vehicle)
                .OrderByDescending(f => f.FuelDate)
                .Take(limit)
                .Select(f => new {
                    Type = "Fuel",
                    f.Id,
                    Description = $"{f.Vehicle.Make} {f.Vehicle.Model}: {f.LitersFilled}L",
                    Status = "Completed",
                    Date = f.FuelDate
                })
                .ToListAsync();

            var recentMaintenance = await _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .OrderByDescending(m => m.ServiceDate)
                .Take(limit)
                .Select(m => new {
                    Type = "Maintenance",
                    m.Id,
                    Description = $"{m.Vehicle.Make} {m.Vehicle.Model}: {m.ServiceType}",
                    m.Status,
                    Date = m.ServiceDate
                })
                .ToListAsync();

            var allActivity = recentTrips.Concat(recentFuel).Concat(recentMaintenance)
                .OrderByDescending(a => a.Date)
                .Take(limit);

            return Ok(allActivity);
        }
    }
}

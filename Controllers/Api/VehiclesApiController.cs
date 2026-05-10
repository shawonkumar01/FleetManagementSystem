using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VehiclesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/VehiclesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetVehicles()
        {
            var vehicles = await _context.Vehicles
                .Select(v => new {
                    v.Id,
                    v.Make,
                    v.Model,
                    v.Year,
                    v.LicensePlate,
                    v.Status,
                    v.Mileage,
                    TripCount = v.Trips.Count,
                    FuelRecordsCount = v.FuelRecords.Count
                })
                .ToListAsync();

            return Ok(vehicles);
        }

        // GET: api/VehiclesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetVehicle(int id)
        {
            var vehicle = await _context.Vehicles
                .Where(v => v.Id == id)
                .Select(v => new {
                    v.Id,
                    v.Make,
                    v.Model,
                    v.Year,
                    v.LicensePlate,
                    v.Status,
                    v.Mileage,
                    Trips = v.Trips.Select(t => new { t.Id, t.Origin, t.Destination, t.Status, t.StartTime }),
                    FuelRecords = v.FuelRecords.Select(f => new { f.Id, f.FuelDate, f.LitersFilled, f.TotalCost })
                })
                .FirstOrDefaultAsync();

            if (vehicle == null) return NotFound();

            return Ok(vehicle);
        }

        // GET: api/VehiclesApi/stats
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetStats()
        {
            var stats = new
            {
                Total = await _context.Vehicles.CountAsync(),
                Active = await _context.Vehicles.CountAsync(v => v.Status == "Active"),
                InMaintenance = await _context.Vehicles.CountAsync(v => v.Status == "InMaintenance"),
                Retired = await _context.Vehicles.CountAsync(v => v.Status == "Retired")
            };

            return Ok(stats);
        }
    }
}

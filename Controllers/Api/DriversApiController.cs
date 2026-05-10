using FleetManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DriversApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DriversApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DriversApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetDrivers()
        {
            var drivers = await _context.Drivers
                .Select(d => new {
                    d.Id,
                    d.FirstName,
                    d.LastName,
                    d.LicenseNumber,
                    d.LicenseExpiry,
                    d.Phone,
                    d.Status,
                    DaysUntilExpiry = (d.LicenseExpiry - DateTime.UtcNow).Days,
                    TripCount = d.Trips.Count
                })
                .ToListAsync();

            return Ok(drivers);
        }

        // GET: api/DriversApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetDriver(int id)
        {
            var driver = await _context.Drivers
                .Where(d => d.Id == id)
                .Select(d => new {
                    d.Id,
                    d.FirstName,
                    d.LastName,
                    d.LicenseNumber,
                    d.LicenseExpiry,
                    d.Phone,
                    d.Status,
                    Trips = d.Trips.Select(t => new { t.Id, t.Origin, t.Destination, t.Status, t.StartTime })
                })
                .FirstOrDefaultAsync();

            if (driver == null) return NotFound();

            return Ok(driver);
        }

        // GET: api/DriversApi/expiring
        [HttpGet("expiring")]
        public async Task<ActionResult<IEnumerable<object>>> GetExpiringLicenses([FromQuery] int days = 30)
        {
            var threshold = DateTime.UtcNow.AddDays(days);
            var drivers = await _context.Drivers
                .Where(d => d.LicenseExpiry <= threshold && d.Status == "Active")
                .Select(d => new {
                    d.Id,
                    d.FirstName,
                    d.LastName,
                    d.LicenseNumber,
                    d.LicenseExpiry,
                    DaysUntilExpiry = (d.LicenseExpiry - DateTime.UtcNow).Days
                })
                .OrderBy(d => d.LicenseExpiry)
                .ToListAsync();

            return Ok(drivers);
        }
    }
}

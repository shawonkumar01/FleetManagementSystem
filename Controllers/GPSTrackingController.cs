using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FleetManagementSystem.Controllers
{
    [Authorize]
    public class GPSTrackingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GPSTrackingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GPSTracking
        public async Task<IActionResult> Index()
        {
            var vehicles = await _context.Vehicles
                .Where(v => v.Status == "Active")
                .ToListAsync();

            var latestLocations = await _context.GPSTracking
                .GroupBy(g => g.VehicleId)
                .Select(g => g.OrderByDescending(gt => gt.Timestamp).FirstOrDefault())
                .ToListAsync();

            ViewBag.Vehicles = vehicles;
            ViewBag.LatestLocations = latestLocations;

            return View();
        }

        // GET: GPSTracking/LiveMap
        public async Task<IActionResult> LiveMap()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.GPSTrackings.OrderByDescending(gt => gt.Timestamp).Take(1))
                .Where(v => v.Status == "Active")
                .ToListAsync();

            return View(vehicles);
        }

        // GET: GPSTracking/History/5
        public async Task<IActionResult> History(int? id, DateTime? from, DateTime? to)
        {
            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
            var toDate = to ?? DateTime.UtcNow;

            var locations = await _context.GPSTracking
                .Where(gt => gt.VehicleId == id && gt.Timestamp >= fromDate && gt.Timestamp <= toDate)
                .OrderByDescending(gt => gt.Timestamp)
                .ToListAsync();

            ViewBag.Vehicle = vehicle;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(locations);
        }

        // GET: GPSTracking/Track/5
        public async Task<IActionResult> Track(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles
                .Include(v => v.GPSTrackings.OrderByDescending(gt => gt.Timestamp).Take(50))
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // POST: GPSTracking/UpdateLocation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLocation(int vehicleId, decimal latitude, decimal longitude, decimal? altitude, decimal? heading, decimal? speed, string? address)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null) return NotFound();

            var gpsTracking = new GPSTracking
            {
                VehicleId = vehicleId,
                Latitude = latitude,
                Longitude = longitude,
                Altitude = altitude,
                Heading = heading,
                Speed = speed,
                Address = address,
                Timestamp = DateTime.UtcNow
            };

            _context.GPSTracking.Add(gpsTracking);
            await _context.SaveChangesAsync();

            // Check geofence alerts
            await CheckGeofenceAlerts(gpsTracking);

            return Json(new { success = true, message = "Location updated successfully" });
        }

        // GET: GPSTracking/GetLatestLocations
        public async Task<IActionResult> GetLatestLocations()
        {
            var latestLocations = await _context.GPSTracking
                .Include(gt => gt.Vehicle)
                .GroupBy(g => g.VehicleId)
                .Select(g => g.OrderByDescending(gt => gt.Timestamp).FirstOrDefault())
                .ToListAsync();

            return Json(latestLocations);
        }

        // GET: GPSTracking/GetVehicleHistory/5
        public async Task<IActionResult> GetVehicleHistory(int id, DateTime from, DateTime to)
        {
            var locations = await _context.GPSTracking
                .Where(gt => gt.VehicleId == id && gt.Timestamp >= from && gt.Timestamp <= to)
                .OrderBy(gt => gt.Timestamp)
                .ToListAsync();

            return Json(locations);
        }

        private async Task CheckGeofenceAlerts(GPSTracking gpsTracking)
        {
            var geofences = await _context.Geofences
                .Where(g => g.IsActive)
                .ToListAsync();

            foreach (var geofence in geofences)
            {
                bool isInside = false;

                if (geofence.Type == "Circle" && geofence.CenterLatitude.HasValue && geofence.CenterLongitude.HasValue && geofence.Radius.HasValue)
                {
                    // Check if point is inside circular geofence
                    var distance = CalculateDistance(
                        gpsTracking.Latitude, gpsTracking.Longitude,
                        geofence.CenterLatitude.Value, geofence.CenterLongitude.Value
                    );
                    isInside = distance <= (double)geofence.Radius.Value;
                }

                // Check if this is a new entry or exit
                var lastAlert = await _context.GeofenceAlerts
                    .Where(ga => ga.GeofenceId == geofence.Id && ga.GPSTrackingId == gpsTracking.Id)
                    .OrderByDescending(ga => ga.AlertTime)
                    .FirstOrDefaultAsync();

                if (lastAlert == null || lastAlert.AlertType != (isInside ? "Enter" : "Exit"))
                {
                    var alert = new GeofenceAlert
                    {
                        GeofenceId = geofence.Id,
                        GPSTrackingId = gpsTracking.Id,
                        AlertType = isInside ? "Enter" : "Exit",
                        Message = $"Vehicle {gpsTracking.Vehicle.Make} {gpsTracking.Vehicle.Model} { (isInside ? "entered" : "exited") } geofence '{geofence.Name}'"
                    };

                    _context.GeofenceAlerts.Add(alert);
                }
            }

            await _context.SaveChangesAsync();
        }

        private double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            var R = 6371000; // Earth's radius in meters
            var dLat = ToRadians((double)(lat2 - lat1));
            var dLon = ToRadians((double)(lon2 - lon1));
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double angle)
        {
            return angle * (Math.PI / 180);
        }
    }
}

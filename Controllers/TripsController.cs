using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Controllers
{
    public class TripsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TripsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trips
        public async Task<IActionResult> Index()
        {
            var trips = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();
            return View(trips);
        }

        // GET: Trips/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null) return NotFound();

            return View(trip);
        }

        // GET: Trips/Create
        public IActionResult Create()
        {
            ViewBag.Vehicles = new SelectList(_context.Vehicles.Where(v => v.Status == "Active"), "Id", "LicensePlate");
            ViewBag.Drivers = new SelectList(_context.Drivers.Where(d => d.Status == "Active"), "Id", "FirstName");
            return View();
        }

        // POST: Trips/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trip trip)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trip);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Trip created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Vehicles = new SelectList(_context.Vehicles.Where(v => v.Status == "Active"), "Id", "LicensePlate");
            ViewBag.Drivers = new SelectList(_context.Drivers.Where(d => d.Status == "Active"), "Id", "FirstName");
            return View(trip);
        }

        // GET: Trips/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _context.Trips.FindAsync(id);
            if (trip == null) return NotFound();

            ViewBag.Vehicles = new SelectList(_context.Vehicles, "Id", "LicensePlate", trip.VehicleId);
            ViewBag.Drivers = new SelectList(_context.Drivers, "Id", "FirstName", trip.DriverId);
            return View(trip);
        }

        // POST: Trips/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Trip trip)
        {
            if (id != trip.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trip);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Trip updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Trips.Any(t => t.Id == trip.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Vehicles = new SelectList(_context.Vehicles, "Id", "LicensePlate", trip.VehicleId);
            ViewBag.Drivers = new SelectList(_context.Drivers, "Id", "FirstName", trip.DriverId);
            return View(trip);
        }

        // GET: Trips/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null) return NotFound();

            return View(trip);
        }

        // POST: Trips/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip != null)
            {
                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Trip deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FleetManagementSystem.Controllers
{
    [Authorize]
    public class GeofenceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GeofenceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Geofence
        public async Task<IActionResult> Index()
        {
            var geofences = await _context.Geofences
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return View(geofences);
        }

        // GET: Geofence/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Geofence/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Geofence geofence)
        {
            if (ModelState.IsValid)
            {
                _context.Add(geofence);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Geofence created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(geofence);
        }

        // GET: Geofence/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var geofence = await _context.Geofences.FindAsync(id);
            if (geofence == null) return NotFound();

            return View(geofence);
        }

        // POST: Geofence/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Geofence geofence)
        {
            if (id != geofence.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(geofence);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Geofence updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GeofenceExists(geofence.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(geofence);
        }

        // GET: Geofence/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var geofence = await _context.Geofences.FindAsync(id);
            if (geofence == null) return NotFound();

            return View(geofence);
        }

        // POST: Geofence/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var geofence = await _context.Geofences.FindAsync(id);
            if (geofence != null)
            {
                _context.Geofences.Remove(geofence);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Geofence deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Geofence/Alerts
        public async Task<IActionResult> Alerts()
        {
            var alerts = await _context.GeofenceAlerts
                .Include(ga => ga.Geofence)
                .Include(ga => ga.GPSTracking)
                .ThenInclude(gt => gt.Vehicle)
                .OrderByDescending(ga => ga.AlertTime)
                .ToListAsync();

            return View(alerts);
        }

        // POST: Geofence/MarkAlertAsRead/5
        [HttpPost]
        public async Task<IActionResult> MarkAlertAsRead(int id)
        {
            var alert = await _context.GeofenceAlerts.FindAsync(id);
            if (alert != null)
            {
                alert.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        // GET: Geofence/GetGeofences
        public async Task<IActionResult> GetGeofences()
        {
            var geofences = await _context.Geofences
                .Where(g => g.IsActive)
                .ToListAsync();

            return Json(geofences);
        }

        private bool GeofenceExists(int id)
        {
            return _context.Geofences.Any(e => e.Id == id);
        }
    }
}

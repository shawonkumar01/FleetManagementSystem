using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Controllers
{
    [Authorize]
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Maintenance
        public async Task<IActionResult> Index()
        {
            var records = await _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .OrderByDescending(m => m.ServiceDate)
                .ToListAsync();
            return View(records);
        }

        // GET: Maintenance/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (record == null) return NotFound();

            return View(record);
        }

        // GET: Maintenance/Create
        public IActionResult Create()
        {
            ViewBag.Vehicles = new SelectList(_context.Vehicles, "Id", "LicensePlate");
            return View();
        }

        // POST: Maintenance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Maintenance maintenance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(maintenance);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Maintenance record added successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Vehicles = new SelectList(_context.Vehicles, "Id", "LicensePlate");
            return View(maintenance);
        }

        // GET: Maintenance/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.MaintenanceRecords.FindAsync(id);
            if (record == null) return NotFound();

            ViewBag.Vehicles = new SelectList(_context.Vehicles, "Id", "LicensePlate", record.VehicleId);
            return View(record);
        }

        // POST: Maintenance/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Maintenance maintenance)
        {
            if (id != maintenance.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(maintenance);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Maintenance record updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.MaintenanceRecords.Any(m => m.Id == maintenance.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Vehicles = new SelectList(_context.Vehicles, "Id", "LicensePlate", maintenance.VehicleId);
            return View(maintenance);
        }

        // GET: Maintenance/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (record == null) return NotFound();

            return View(record);
        }

        // POST: Maintenance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var record = await _context.MaintenanceRecords.FindAsync(id);
            if (record != null)
            {
                _context.MaintenanceRecords.Remove(record);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Maintenance record deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
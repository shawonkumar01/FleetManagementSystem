using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Controllers
{
    public class FuelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FuelController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Fuel
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Fuel Logs";

            var records = await _context.FuelRecords
                .Include(f => f.Vehicle)
                .OrderByDescending(f => f.FuelDate)
                .ToListAsync();

            ViewBag.TotalLiters = records.Sum(f => f.LitersFilled);
            ViewBag.TotalFuelCost = records.Sum(f => f.TotalCost);
            ViewBag.TotalRecords = records.Count;
            ViewBag.AvgPerLiter = records.Any() ? records.Average(f => (double)f.PricePerLiter) : 0;

            return View(records);
        }

        // GET: Fuel/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.FuelRecords
                .Include(f => f.Vehicle)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (record == null) return NotFound();

            return View(record);
        }

        // GET: Fuel/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Fuel Record";
            ViewBag.Vehicles = new SelectList(_context.Vehicles, "Id", "LicensePlate");
            return View();
        }

        // POST: Fuel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FuelRecord fuel)
        {
            if (ModelState.IsValid)
            {
                // Auto-calculate total cost if not provided
                if (fuel.TotalCost == 0)
                    fuel.TotalCost = (decimal)fuel.LitersFilled * fuel.PricePerLiter;

                _context.Add(fuel);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Fuel record added successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Vehicles = new SelectList(_context.Vehicles, "Id", "LicensePlate");
            return View(fuel);
        }

        // GET: Fuel/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.FuelRecords.FindAsync(id);
            if (record == null) return NotFound();

            ViewData["Title"] = "Edit Fuel Record";
            ViewBag.Vehicles = new SelectList(_context.Vehicles, "Id", "LicensePlate", record.VehicleId);
            return View(record);
        }

        // POST: Fuel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FuelRecord fuel)
        {
            if (id != fuel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (fuel.TotalCost == 0)
                        fuel.TotalCost = (decimal)fuel.LitersFilled * fuel.PricePerLiter;

                    _context.Update(fuel);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Fuel record updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.FuelRecords.Any(f => f.Id == fuel.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Vehicles = new SelectList(_context.Vehicles, "Id", "LicensePlate", fuel.VehicleId);
            return View(fuel);
        }

        // GET: Fuel/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.FuelRecords
                .Include(f => f.Vehicle)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (record == null) return NotFound();

            return View(record);
        }

        // POST: Fuel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var record = await _context.FuelRecords.FindAsync(id);
            if (record != null)
            {
                _context.FuelRecords.Remove(record);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Fuel record deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
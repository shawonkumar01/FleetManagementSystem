using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Controllers
{
    [Authorize]
    public class DriversController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DriversController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Drivers
        public async Task<IActionResult> Index()
        {
            var drivers = await _context.Drivers.ToListAsync();
            return View(drivers);
        }

        // GET: Drivers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var driver = await _context.Drivers
                .Include(d => d.Trips)
                    .ThenInclude(t => t.Vehicle)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (driver == null) return NotFound();

            return View(driver);
        }

        // GET: Drivers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Drivers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Driver driver)
        {
            if (ModelState.IsValid)
            {
                _context.Add(driver);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Driver added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(driver);
        }

        // GET: Drivers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null) return NotFound();

            return View(driver);
        }

        // POST: Drivers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Driver driver)
        {
            if (id != driver.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(driver);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Driver updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Drivers.Any(d => d.Id == driver.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(driver);
        }

        // GET: Drivers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == id);

            if (driver == null) return NotFound();

            return View(driver);
        }

        // POST: Drivers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver != null)
            {
                _context.Drivers.Remove(driver);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Driver deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
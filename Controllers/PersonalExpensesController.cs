using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FleetManagementSystem.Controllers
{
    [Authorize]
    public class PersonalExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PersonalExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? vehicleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = _context.PersonalExpenses
                .Include(pe => pe.PersonalVehicle)
                .Where(pe => pe.PersonalVehicle.UserId == userId)
                .AsQueryable();

            if (vehicleId.HasValue)
            {
                query = query.Where(pe => pe.PersonalVehicleId == vehicleId);
                ViewBag.VehicleId = vehicleId;
            }

            var expenses = await query
                .OrderByDescending(pe => pe.ExpenseDate)
                .ToListAsync();

            return View(expenses);
        }

        public async Task<IActionResult> Create(int? vehicleId, string? type)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vehicles = await _context.PersonalVehicles
                .Where(pv => pv.UserId == userId)
                .ToListAsync();

            ViewBag.Vehicles = vehicles;
            ViewBag.PreselectedVehicleId = vehicleId;
            ViewBag.PreselectedType = type;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonalExpense expense)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vehicle = await _context.PersonalVehicles
                .FirstOrDefaultAsync(pv => pv.Id == expense.PersonalVehicleId && pv.UserId == userId);

            if (vehicle == null)
            {
                ModelState.AddModelError("", "Invalid vehicle selected.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(expense);
                
                // Update odometer if provided and higher than current
                if (expense.OdometerReading.HasValue && expense.OdometerReading > vehicle.CurrentOdometer)
                {
                    vehicle.CurrentOdometer = expense.OdometerReading.Value;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Expense recorded successfully!";
                return RedirectToAction("Details", "PersonalVehicles", new { id = expense.PersonalVehicleId });
            }

            ViewBag.Vehicles = await _context.PersonalVehicles
                .Where(pv => pv.UserId == userId)
                .ToListAsync();
            return View(expense);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var expense = await _context.PersonalExpenses
                .Include(pe => pe.PersonalVehicle)
                .FirstOrDefaultAsync(pe => pe.Id == id && pe.PersonalVehicle.UserId == userId);

            if (expense == null) return NotFound();

            return View(expense);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var expense = await _context.PersonalExpenses
                .Include(pe => pe.PersonalVehicle)
                .FirstOrDefaultAsync(pe => pe.Id == id && pe.PersonalVehicle.UserId == userId);

            if (expense != null)
            {
                var vehicleId = expense.PersonalVehicleId;
                _context.PersonalExpenses.Remove(expense);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Expense deleted successfully!";
                return RedirectToAction("Details", "PersonalVehicles", new { id = vehicleId });
            }
            return RedirectToAction("Index", "PersonalVehicles");
        }

        // Expense Report
        public async Task<IActionResult> Report(int vehicleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vehicle = await _context.PersonalVehicles
                .FirstOrDefaultAsync(pv => pv.Id == vehicleId && pv.UserId == userId);

            if (vehicle == null) return NotFound();

            var expenses = await _context.PersonalExpenses
                .Where(pe => pe.PersonalVehicleId == vehicleId)
                .OrderBy(pe => pe.ExpenseDate)
                .ToListAsync();

            // Calculate monthly summaries
            var monthlyData = expenses
                .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Total = g.Sum(e => e.Amount),
                    Fuel = g.Where(e => e.ExpenseType == "Fuel").Sum(e => e.Amount),
                    Maintenance = g.Where(e => e.ExpenseType == "Maintenance" || e.ExpenseType == "Service").Sum(e => e.Amount),
                    Other = g.Where(e => e.ExpenseType != "Fuel" && e.ExpenseType != "Maintenance" && e.ExpenseType != "Service").Sum(e => e.Amount)
                })
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .ToList();

            ViewBag.Vehicle = vehicle;
            ViewBag.MonthlyData = monthlyData;
            ViewBag.TotalSpent = expenses.Sum(e => e.Amount);
            ViewBag.TotalFuel = expenses.Where(e => e.ExpenseType == "Fuel").Sum(e => e.Amount);
            ViewBag.TotalMaintenance = expenses.Where(e => e.ExpenseType == "Maintenance" || e.ExpenseType == "Service").Sum(e => e.Amount);
            ViewBag.TotalLiters = expenses.Where(e => e.ExpenseType == "Fuel").Sum(e => e.Liters ?? 0);

            return View(expenses);
        }
    }
}

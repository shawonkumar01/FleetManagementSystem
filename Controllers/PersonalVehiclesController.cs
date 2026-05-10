using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FleetManagementSystem.Controllers
{
    [Authorize]
    public class PersonalVehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PersonalVehiclesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vehicles = await _context.PersonalVehicles
                .Where(pv => pv.UserId == userId)
                .Include(pv => pv.Expenses)
                .OrderByDescending(pv => pv.CreatedAt)
                .ToListAsync();

            return View(vehicles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonalVehicle vehicle)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            vehicle.UserId = userId;

            if (ModelState.IsValid)
            {
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Vehicle added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vehicle = await _context.PersonalVehicles
                .Include(pv => pv.Documents)
                .Include(pv => pv.Expenses.OrderByDescending(e => e.ExpenseDate))
                .FirstOrDefaultAsync(pv => pv.Id == id && pv.UserId == userId);

            if (vehicle == null) return NotFound();

            // Calculate monthly stats
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            
            ViewBag.MonthlyFuelCost = vehicle.Expenses
                .Where(e => e.ExpenseType == "Fuel" && e.ExpenseDate.Month == currentMonth && e.ExpenseDate.Year == currentYear)
                .Sum(e => e.Amount);

            ViewBag.MonthlyMaintenanceCost = vehicle.Expenses
                .Where(e => (e.ExpenseType == "Maintenance" || e.ExpenseType == "Service") && e.ExpenseDate.Month == currentMonth && e.ExpenseDate.Year == currentYear)
                .Sum(e => e.Amount);

            ViewBag.TotalSpent = vehicle.Expenses.Sum(e => e.Amount);
            ViewBag.TotalFuelLiters = vehicle.Expenses
                .Where(e => e.ExpenseType == "Fuel")
                .Sum(e => e.Liters ?? 0);

            return View(vehicle);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vehicle = await _context.PersonalVehicles
                .FirstOrDefaultAsync(pv => pv.Id == id && pv.UserId == userId);

            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PersonalVehicle vehicle)
        {
            if (id != vehicle.Id) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingVehicle = await _context.PersonalVehicles
                .FirstOrDefaultAsync(pv => pv.Id == id && pv.UserId == userId);

            if (existingVehicle == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingVehicle.Name = vehicle.Name;
                existingVehicle.Make = vehicle.Make;
                existingVehicle.Model = vehicle.Model;
                existingVehicle.Year = vehicle.Year;
                existingVehicle.LicensePlate = vehicle.LicensePlate;
                existingVehicle.VIN = vehicle.VIN;
                existingVehicle.CurrentOdometer = vehicle.CurrentOdometer;
                existingVehicle.PurchaseDate = vehicle.PurchaseDate;
                existingVehicle.PurchasePrice = vehicle.PurchasePrice;
                existingVehicle.Notes = vehicle.Notes;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Vehicle updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vehicle = await _context.PersonalVehicles
                .FirstOrDefaultAsync(pv => pv.Id == id && pv.UserId == userId);

            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vehicle = await _context.PersonalVehicles
                .FirstOrDefaultAsync(pv => pv.Id == id && pv.UserId == userId);

            if (vehicle != null)
            {
                _context.PersonalVehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Vehicle deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: PersonalVehicles/ExpenseReport/5
        public async Task<IActionResult> ExpenseReport(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vehicle = await _context.PersonalVehicles
                .FirstOrDefaultAsync(pv => pv.Id == id && pv.UserId == userId);

            if (vehicle == null) return NotFound();

            var expenses = await _context.PersonalExpenses
                .Where(pe => pe.PersonalVehicleId == id)
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

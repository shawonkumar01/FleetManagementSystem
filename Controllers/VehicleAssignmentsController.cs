using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class VehicleAssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehicleAssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var assignments = await _context.VehicleAssignments
                .Include(va => va.Vehicle)
                .Include(va => va.Driver)
                .OrderByDescending(va => va.AssignedDate)
                .ToListAsync();
            return View(assignments);
        }

        public IActionResult Create()
        {
            ViewBag.Vehicles = new SelectList(
                _context.Vehicles.Where(v => v.Status == "Active"),
                "Id", "LicensePlate");
            ViewBag.Drivers = new SelectList(
                _context.Drivers.Where(d => d.Status == "Active"),
                "Id", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleAssignment assignment)
        {
            // Check if vehicle already has an active assignment
            var existingAssignment = await _context.VehicleAssignments
                .FirstOrDefaultAsync(va => va.VehicleId == assignment.VehicleId && va.Status == "Active");

            if (existingAssignment != null)
            {
                ModelState.AddModelError("", "This vehicle already has an active assignment. Please end the current assignment first.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(assignment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Vehicle assigned successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Vehicles = new SelectList(_context.Vehicles.Where(v => v.Status == "Active"), "Id", "LicensePlate");
            ViewBag.Drivers = new SelectList(_context.Drivers.Where(d => d.Status == "Active"), "Id", "FirstName");
            return View(assignment);
        }

        public async Task<IActionResult> EndAssignment(int id)
        {
            var assignment = await _context.VehicleAssignments.FindAsync(id);
            if (assignment == null) return NotFound();

            assignment.Status = "Ended";
            assignment.EndDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Assignment ended successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.VehicleAssignments
                .Include(va => va.Vehicle)
                .Include(va => va.Driver)
                .FirstOrDefaultAsync(va => va.Id == id);

            if (assignment == null) return NotFound();

            return View(assignment);
        }
    }
}

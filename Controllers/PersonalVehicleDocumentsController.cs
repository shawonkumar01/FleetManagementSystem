using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FleetManagementSystem.Controllers
{
    [Authorize]
    public class PersonalVehicleDocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PersonalVehicleDocumentsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Create(int? vehicleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vehicles = await _context.PersonalVehicles
                .Where(pv => pv.UserId == userId)
                .ToListAsync();

            ViewBag.Vehicles = vehicles;
            ViewBag.PreselectedVehicleId = vehicleId;

            ViewData["Layout"] = "_UserLayout";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonalVehicleDocument document, IFormFile? file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vehicle = await _context.PersonalVehicles
                .FirstOrDefaultAsync(pv => pv.Id == document.PersonalVehicleId && pv.UserId == userId);

            if (vehicle == null)
            {
                ModelState.AddModelError("", "Invalid vehicle selected.");
            }

            if (file != null && file.Length > 0)
            {
                // Validate file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "File size must be less than 10MB.");
                }

                // Save file
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                document.FilePath = Path.Combine("uploads", "documents", uniqueFileName).Replace("\\", "/");
                document.OriginalFileName = file.FileName;
                document.FileSize = file.Length;
            }

            if (ModelState.IsValid)
            {
                _context.Add(document);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Document uploaded successfully!";
                return RedirectToAction("Details", "PersonalVehicles", new { id = document.PersonalVehicleId });
            }

            ViewBag.Vehicles = await _context.PersonalVehicles
                .Where(pv => pv.UserId == userId)
                .ToListAsync();
            return View(document);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var document = await _context.PersonalVehicleDocuments
                .Include(d => d.PersonalVehicle)
                .FirstOrDefaultAsync(d => d.Id == id && d.PersonalVehicle.UserId == userId);

            if (document == null) return NotFound();

            ViewData["Layout"] = "_UserLayout";
            return View(document);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var document = await _context.PersonalVehicleDocuments
                .Include(d => d.PersonalVehicle)
                .FirstOrDefaultAsync(d => d.Id == id && d.PersonalVehicle.UserId == userId);

            if (document != null)
            {
                var vehicleId = document.PersonalVehicleId;

                // Delete physical file
                if (!string.IsNullOrEmpty(document.FilePath))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, document.FilePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.PersonalVehicleDocuments.Remove(document);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Document deleted successfully!";
                return RedirectToAction("Details", "PersonalVehicles", new { id = vehicleId });
            }
            return RedirectToAction("Index", "PersonalVehicles");
        }
    }
}

using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FleetManagementSystem.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DocumentsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Documents
        public async Task<IActionResult> Index(string? category, string? search, int? vehicleId, int? driverId, string? status)
        {
            var query = _context.Documents
                .Include(d => d.Vehicle)
                .Include(d => d.Driver)
                .Where(d => d.ParentDocumentId == null) // Only show main documents, not versions
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(category))
                query = query.Where(d => d.Category == category);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => d.Title.Contains(search) || 
                    (d.Description != null && d.Description.Contains(search)) ||
                    (d.Tags != null && d.Tags.Contains(search)) ||
                    (d.DocumentNumber != null && d.DocumentNumber.Contains(search)));

            if (vehicleId.HasValue)
                query = query.Where(d => d.VehicleId == vehicleId);

            if (driverId.HasValue)
                query = query.Where(d => d.DriverId == driverId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(d => d.Status == status);

            var documents = await query
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            // Get statistics
            ViewBag.TotalDocuments = await _context.Documents.CountAsync(d => d.ParentDocumentId == null);
            ViewBag.ActiveDocuments = await _context.Documents.CountAsync(d => d.ParentDocumentId == null && d.Status == "Active");
            ViewBag.ExpiringSoon = await _context.Documents
                .CountAsync(d => d.ParentDocumentId == null && 
                    d.ExpiryDate.HasValue && 
                    d.ExpiryDate.Value <= DateTime.UtcNow.AddDays(30) && 
                    d.ExpiryDate.Value > DateTime.UtcNow);
            ViewBag.ExpiredDocuments = await _context.Documents
                .CountAsync(d => d.ParentDocumentId == null && 
                    d.ExpiryDate.HasValue && 
                    d.ExpiryDate.Value <= DateTime.UtcNow);

            // Get filter options
            ViewBag.Categories = await _context.Documents
                .Where(d => d.ParentDocumentId == null)
                .Select(d => d.Category)
                .Distinct()
                .ToListAsync();
            ViewBag.Vehicles = await _context.Vehicles.ToListAsync();
            ViewBag.Drivers = await _context.Drivers.ToListAsync();

            // Current filters
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentVehicleId = vehicleId;
            ViewBag.CurrentDriverId = driverId;
            ViewBag.CurrentStatus = status;

            return View(documents);
        }

        // GET: Documents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var document = await _context.Documents
                .Include(d => d.Vehicle)
                .Include(d => d.Driver)
                .Include(d => d.Versions)
                .Include(d => d.ParentDocument)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null) return NotFound();

            // Log access
            await LogDocumentAccess(document.Id, "View");

            // Get access history
            ViewBag.AccessHistory = await _context.DocumentAccessLogs
                .Where(l => l.DocumentId == id)
                .OrderByDescending(l => l.ActionTime)
                .Take(20)
                .ToListAsync();

            return View(document);
        }

        // GET: Documents/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
            ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
            ViewBag.Categories = GetDocumentCategories();

            return View();
        }

        // POST: Documents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Document document, IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select a file to upload");
            }

            if (ModelState.IsValid)
            {
                // Handle file upload
                if (file != null && file.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Create subfolder by category
                    var categoryFolder = Path.Combine(uploadsFolder, document.Category.Replace(" ", "_"));
                    if (!Directory.Exists(categoryFolder))
                        Directory.CreateDirectory(categoryFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(categoryFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    document.FilePath = $"/uploads/documents/{document.Category.Replace(" ", "_")}/{uniqueFileName}";
                    document.FileName = file.FileName;
                    document.FileSize = file.Length;
                    document.FileExtension = Path.GetExtension(file.FileName)?.ToLower();
                    document.DocumentType = GetDocumentType(document.FileExtension);
                }

                document.UploadedBy = User.Identity?.Name;
                document.UploadedAt = DateTime.UtcNow;

                // Check and update status based on expiry date
                if (document.ExpiryDate.HasValue && document.ExpiryDate.Value <= DateTime.UtcNow)
                {
                    document.Status = "Expired";
                }

                _context.Add(document);
                await _context.SaveChangesAsync();

                // Log upload
                await LogDocumentAccess(document.Id, "Upload");

                TempData["Success"] = "Document uploaded successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
            ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
            ViewBag.Categories = GetDocumentCategories();

            return View(document);
        }

        // GET: Documents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound();

            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
            ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
            ViewBag.Categories = GetDocumentCategories();

            return View(document);
        }

        // POST: Documents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Document document, IFormFile? newFile)
        {
            if (id != document.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingDoc = await _context.Documents.FindAsync(id);
                    if (existingDoc == null) return NotFound();

                    // If new file uploaded, create a new version
                    if (newFile != null && newFile.Length > 0)
                    {
                        // Create new version document
                        var newVersion = new Document
                        {
                            Title = document.Title,
                            Description = document.Description,
                            Category = document.Category,
                            VehicleId = document.VehicleId,
                            DriverId = document.DriverId,
                            IssueDate = document.IssueDate,
                            ExpiryDate = document.ExpiryDate,
                            DocumentNumber = document.DocumentNumber,
                            IssuingAuthority = document.IssuingAuthority,
                            Status = document.Status,
                            IsConfidential = document.IsConfidential,
                            Tags = document.Tags,
                            ParentDocumentId = existingDoc.ParentDocumentId ?? existingDoc.Id,
                            Version = existingDoc.Version + 1,
                            UploadedBy = User.Identity?.Name,
                            UploadedAt = DateTime.UtcNow
                        };

                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                        var categoryFolder = Path.Combine(uploadsFolder, document.Category.Replace(" ", "_"));
                        if (!Directory.Exists(categoryFolder))
                            Directory.CreateDirectory(categoryFolder);

                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(newFile.FileName)}";
                        var filePath = Path.Combine(categoryFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await newFile.CopyToAsync(fileStream);
                        }

                        newVersion.FilePath = $"/uploads/documents/{document.Category.Replace(" ", "_")}/{uniqueFileName}";
                        newVersion.FileName = newFile.FileName;
                        newVersion.FileSize = newFile.Length;
                        newVersion.FileExtension = Path.GetExtension(newFile.FileName)?.ToLower();
                        newVersion.DocumentType = GetDocumentType(newVersion.FileExtension);

                        _context.Add(newVersion);
                        await _context.SaveChangesAsync();

                        // Log new version upload
                        await LogDocumentAccess(newVersion.Id, "Upload (New Version)");

                        TempData["Success"] = "New version uploaded successfully!";
                        return RedirectToAction(nameof(Details), new { id = newVersion.Id });
                    }
                    else
                    {
                        // Update metadata only
                        existingDoc.Title = document.Title;
                        existingDoc.Description = document.Description;
                        existingDoc.Category = document.Category;
                        existingDoc.VehicleId = document.VehicleId;
                        existingDoc.DriverId = document.DriverId;
                        existingDoc.IssueDate = document.IssueDate;
                        existingDoc.ExpiryDate = document.ExpiryDate;
                        existingDoc.DocumentNumber = document.DocumentNumber;
                        existingDoc.IssuingAuthority = document.IssuingAuthority;
                        existingDoc.Status = document.Status;
                        existingDoc.IsConfidential = document.IsConfidential;
                        existingDoc.Tags = document.Tags;
                        existingDoc.LastModifiedBy = User.Identity?.Name;
                        existingDoc.LastModifiedAt = DateTime.UtcNow;

                        // Update status if expiry date changed
                        if (existingDoc.ExpiryDate.HasValue && existingDoc.ExpiryDate.Value <= DateTime.UtcNow)
                        {
                            existingDoc.Status = "Expired";
                        }

                        await _context.SaveChangesAsync();
                        await LogDocumentAccess(existingDoc.Id, "Update");

                        TempData["Success"] = "Document updated successfully!";
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
            ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
            ViewBag.Categories = GetDocumentCategories();

            return View(document);
        }

        // GET: Documents/Download/5
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null) return NotFound();

            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound();

            var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "File not found on server";
                return RedirectToAction(nameof(Index));
            }

            // Update download statistics
            document.DownloadCount++;
            document.LastDownloadedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Log download
            await LogDocumentAccess(document.Id, "Download");

            var mimeType = GetMimeType(document.FileExtension);
            return PhysicalFile(filePath, mimeType, document.FileName);
        }

        // GET: Documents/View/5
        public async Task<IActionResult> View(int? id)
        {
            if (id == null) return NotFound();

            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound();

            var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "File not found on server";
                return RedirectToAction(nameof(Index));
            }

            // Log view
            await LogDocumentAccess(document.Id, "View");

            var mimeType = GetMimeType(document.FileExtension);
            return PhysicalFile(filePath, mimeType);
        }

        // GET: Documents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var document = await _context.Documents
                .Include(d => d.Vehicle)
                .Include(d => d.Driver)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null) return NotFound();

            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document != null)
            {
                // Soft delete - just mark as archived
                document.Status = "Archived";
                document.LastModifiedBy = User.Identity?.Name;
                document.LastModifiedAt = DateTime.UtcNow;

                await LogDocumentAccess(document.Id, "Delete (Archived)");
                await _context.SaveChangesAsync();

                TempData["Success"] = "Document archived successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Documents/Expiring
        public async Task<IActionResult> Expiring(int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(days);

            var expiringDocs = await _context.Documents
                .Include(d => d.Vehicle)
                .Include(d => d.Driver)
                .Where(d => d.ParentDocumentId == null &&
                    d.ExpiryDate.HasValue &&
                    d.ExpiryDate.Value <= cutoffDate &&
                    d.ExpiryDate.Value > DateTime.UtcNow &&
                    d.Status == "Active")
                .OrderBy(d => d.ExpiryDate)
                .ToListAsync();

            ViewBag.Days = days;
            ViewBag.Count = expiringDocs.Count;

            return View(expiringDocs);
        }

        // GET: Documents/Expired
        public async Task<IActionResult> Expired()
        {
            var expiredDocs = await _context.Documents
                .Include(d => d.Vehicle)
                .Include(d => d.Driver)
                .Where(d => d.ParentDocumentId == null &&
                    d.ExpiryDate.HasValue &&
                    d.ExpiryDate.Value <= DateTime.UtcNow)
                .OrderByDescending(d => d.ExpiryDate)
                .ToListAsync();

            return View(expiredDocs);
        }

        // GET: Documents/GetCategories
        public IActionResult GetCategories()
        {
            return Json(GetDocumentCategories());
        }

        private async Task LogDocumentAccess(int documentId, string action)
        {
            var log = new DocumentAccessLog
            {
                DocumentId = documentId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown",
                UserName = User.Identity?.Name ?? "Unknown",
                Action = action,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                ActionTime = DateTime.UtcNow
            };

            _context.DocumentAccessLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        private List<string> GetDocumentCategories()
        {
            return new List<string>
            {
                "Vehicle Registration",
                "Insurance Policy",
                "Driver License",
                "Maintenance Record",
                "Fuel Receipt",
                "Toll Receipt",
                "Parking Receipt",
                "Trip Document",
                "Invoice",
                "Contract",
                "Certificate",
                "Permit",
                "Inspection Report",
                "Warranty Document",
                "Owner Manual",
                "Other"
            };
        }

        private string GetDocumentType(string? extension)
        {
            return extension?.ToLower() switch
            {
                ".pdf" => "PDF",
                ".doc" or ".docx" => "Word",
                ".xls" or ".xlsx" => "Excel",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "Image",
                ".txt" => "Text",
                ".zip" or ".rar" => "Archive",
                _ => "Other"
            };
        }

        private string GetMimeType(string? extension)
        {
            return extension?.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }
    }
}

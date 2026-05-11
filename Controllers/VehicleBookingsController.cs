using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FleetManagementSystem.Controllers
{
    [Authorize]
    public class VehicleBookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehicleBookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VehicleBookings
        public async Task<IActionResult> Index(string? status, int? vehicleId, DateTime? fromDate, DateTime? toDate, string? priority)
        {
            var query = _context.VehicleBookings
                .Include(b => b.Vehicle)
                .Include(b => b.Driver)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
                query = query.Where(b => b.Status == status);

            if (vehicleId.HasValue)
                query = query.Where(b => b.VehicleId == vehicleId);

            if (fromDate.HasValue)
                query = query.Where(b => b.StartTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(b => b.EndTime <= toDate.Value);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(b => b.Priority == priority);

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // Statistics
            ViewBag.TotalBookings = await _context.VehicleBookings.CountAsync();
            ViewBag.PendingBookings = await _context.VehicleBookings.CountAsync(b => b.Status == "Pending");
            ViewBag.ConfirmedBookings = await _context.VehicleBookings.CountAsync(b => b.Status == "Confirmed");
            ViewBag.TodayBookings = await _context.VehicleBookings
                .CountAsync(b => b.StartTime.Date == DateTime.UtcNow.Date && b.Status != "Cancelled");

            // Filter options
            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentVehicleId = vehicleId;
            ViewBag.CurrentFromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPriority = priority;

            return View(bookings);
        }

        // GET: VehicleBookings/Calendar
        public async Task<IActionResult> Calendar(int? vehicleId, DateTime? month)
        {
            var selectedMonth = month ?? DateTime.UtcNow;
            var startOfMonth = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var query = _context.VehicleBookings
                .Include(b => b.Vehicle)
                .Include(b => b.Driver)
                .Where(b => b.StartTime <= endOfMonth && b.EndTime >= startOfMonth && b.Status != "Cancelled")
                .AsQueryable();

            if (vehicleId.HasValue)
                query = query.Where(b => b.VehicleId == vehicleId);

            var bookings = await query.ToListAsync();

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
            ViewBag.CurrentVehicleId = vehicleId;

            return View(bookings);
        }

        // GET: VehicleBookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.VehicleBookings
                .Include(b => b.Vehicle)
                .Include(b => b.Driver)
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();

            // Get conflicting bookings
            ViewBag.Conflicts = await _context.VehicleBookings
                .Where(b => b.VehicleId == booking.VehicleId &&
                    b.Id != booking.Id &&
                    b.Status != "Cancelled" &&
                    b.StartTime < booking.EndTime &&
                    b.EndTime > booking.StartTime)
                .ToListAsync();

            return View(booking);
        }

        // GET: VehicleBookings/Create
        public async Task<IActionResult> Create(int? vehicleId)
        {
            ViewBag.Vehicles = await _context.Vehicles
                .Where(v => v.Status == "Active")
                .ToListAsync();

            ViewBag.Drivers = await _context.Drivers
                .Where(d => d.Status == "Active")
                .ToListAsync();

            var booking = new VehicleBooking();
            if (vehicleId.HasValue)
                booking.VehicleId = vehicleId.Value;

            // Pre-fill requester info
            booking.RequestedBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "Unknown";
            booking.RequesterName = User.Identity?.Name;

            return View(booking);
        }

        // POST: VehicleBookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleBooking booking)
        {
            if (ModelState.IsValid)
            {
                // Check for conflicts
                var conflicts = await _context.VehicleBookings
                    .Where(b => b.VehicleId == booking.VehicleId &&
                        b.Status != "Cancelled" &&
                        b.Status != "Rejected" &&
                        b.StartTime < booking.EndTime &&
                        b.EndTime > booking.StartTime)
                    .ToListAsync();

                if (conflicts.Any())
                {
                    ModelState.AddModelError("", "This vehicle is already booked for the selected time period.");
                    ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
                    ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
                    ViewBag.ConflictingBookings = conflicts;
                    return View(booking);
                }

                // Generate booking reference
                booking.BookingReference = await GenerateBookingReference();
                booking.Status = "Pending";
                booking.CreatedBy = User.Identity?.Name;
                booking.CreatedAt = DateTime.UtcNow;

                // Auto-approve if user is Admin or Manager
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    booking.Status = "Confirmed";
                    booking.ApprovedBy = User.Identity?.Name;
                    booking.ApprovedAt = DateTime.UtcNow;
                }

                _context.Add(booking);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Booking {booking.BookingReference} created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
            ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
            return View(booking);
        }

        // GET: VehicleBookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.VehicleBookings.FindAsync(id);
            if (booking == null) return NotFound();

            // Only allow editing if not in progress or completed
            if (booking.Status == "InProgress" || booking.Status == "Completed")
            {
                TempData["Error"] = "Cannot edit bookings that are in progress or completed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
            ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();

            return View(booking);
        }

        // POST: VehicleBookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehicleBooking booking)
        {
            if (id != booking.Id) return NotFound();

            if (booking.Status == "InProgress" || booking.Status == "Completed")
            {
                TempData["Error"] = "Cannot edit bookings that are in progress or completed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.VehicleBookings.FindAsync(id);
                    if (existing == null) return NotFound();

                    // Check for conflicts if dates changed
                    if (existing.StartTime != booking.StartTime || existing.EndTime != booking.EndTime || existing.VehicleId != booking.VehicleId)
                    {
                        var conflicts = await _context.VehicleBookings
                            .Where(b => b.VehicleId == booking.VehicleId &&
                                b.Id != booking.Id &&
                                b.Status != "Cancelled" &&
                                b.Status != "Rejected" &&
                                b.StartTime < booking.EndTime &&
                                b.EndTime > booking.StartTime)
                            .ToListAsync();

                        if (conflicts.Any())
                        {
                            ModelState.AddModelError("", "This vehicle is already booked for the selected time period.");
                            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
                            ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
                            return View(booking);
                        }
                    }

                    existing.Purpose = booking.Purpose;
                    existing.TripDescription = booking.TripDescription;
                    existing.PickupLocation = booking.PickupLocation;
                    existing.Destination = booking.Destination;
                    existing.VehicleId = booking.VehicleId;
                    existing.DriverId = booking.DriverId;
                    existing.StartTime = booking.StartTime;
                    existing.EndTime = booking.EndTime;
                    existing.PassengerCount = booking.PassengerCount;
                    existing.PassengerNames = booking.PassengerNames;
                    existing.Priority = booking.Priority;
                    existing.SpecialRequirements = booking.SpecialRequirements;
                    existing.RequiresLuggageSpace = booking.RequiresLuggageSpace;
                    existing.RequiresAirConditioning = booking.RequiresAirConditioning;
                    existing.RequiresWheelchairAccess = booking.RequiresWheelchairAccess;
                    existing.RequiresChildSeat = booking.RequiresChildSeat;
                    existing.LastModifiedAt = DateTime.UtcNow;
                    existing.LastModifiedBy = User.Identity?.Name;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Booking updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
            ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
            return View(booking);
        }

        // POST: VehicleBookings/Approve/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Approve(int id, string? notes)
        {
            var booking = await _context.VehicleBookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status != "Pending" && booking.Status != "Approved")
            {
                TempData["Error"] = "Only pending bookings can be approved.";
                return RedirectToAction(nameof(Details), new { id });
            }

            booking.Status = "Confirmed";
            booking.ApprovedBy = User.Identity?.Name;
            booking.ApprovedAt = DateTime.UtcNow;
            booking.ApprovalNotes = notes;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Booking {booking.BookingReference} approved!";

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: VehicleBookings/Reject/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                TempData["Error"] = "Rejection reason is required.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var booking = await _context.VehicleBookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status != "Pending" && booking.Status != "Approved")
            {
                TempData["Error"] = "Only pending bookings can be rejected.";
                return RedirectToAction(nameof(Details), new { id });
            }

            booking.Status = "Rejected";
            booking.RejectedBy = User.Identity?.Name;
            booking.RejectedAt = DateTime.UtcNow;
            booking.RejectionReason = reason;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Booking {booking.BookingReference} rejected.";

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: VehicleBookings/Cancel/5
        [HttpPost]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            var booking = await _context.VehicleBookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status == "Completed" || booking.Status == "Cancelled")
            {
                TempData["Error"] = "Cannot cancel completed or already cancelled bookings.";
                return RedirectToAction(nameof(Details), new { id });
            }

            booking.Status = "Cancelled";
            booking.CancelledBy = User.Identity?.Name;
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationReason = reason;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Booking {booking.BookingReference} cancelled.";

            return RedirectToAction(nameof(Index));
        }

        // POST: VehicleBookings/CheckOut/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Driver")]
        public async Task<IActionResult> CheckOut(int id, int odometerReading, string? condition)
        {
            var booking = await _context.VehicleBookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status != "Confirmed")
            {
                TempData["Error"] = "Booking must be confirmed before checkout.";
                return RedirectToAction(nameof(Details), new { id });
            }

            booking.Status = "InProgress";
            booking.CheckedOutAt = DateTime.UtcNow;
            booking.CheckedOutBy = User.Identity?.Name;
            booking.StartOdometer = odometerReading;
            booking.CheckoutCondition = condition;
            booking.ActualStartTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Booking {booking.BookingReference} checked out!";

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: VehicleBookings/CheckIn/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Driver")]
        public async Task<IActionResult> CheckIn(int id, int odometerReading, string? condition, decimal? actualCost)
        {
            var booking = await _context.VehicleBookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status != "InProgress")
            {
                TempData["Error"] = "Booking must be in progress before check-in.";
                return RedirectToAction(nameof(Details), new { id });
            }

            booking.Status = "Completed";
            booking.CheckedInAt = DateTime.UtcNow;
            booking.CheckedInBy = User.Identity?.Name;
            booking.EndOdometer = odometerReading;
            booking.CheckinCondition = condition;
            booking.ActualEndTime = DateTime.UtcNow;
            booking.ActualCost = actualCost;

            if (booking.StartOdometer.HasValue)
            {
                booking.ActualDistanceKm = odometerReading - booking.StartOdometer.Value;
            }

            if (booking.ActualStartTime.HasValue)
            {
                booking.ActualDurationHours = (decimal)(booking.ActualEndTime.Value - booking.ActualStartTime.Value).TotalHours;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Booking {booking.BookingReference} completed!";

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: VehicleBookings/Availability
        public async Task<IActionResult> Availability(DateTime? date, int? vehicleId)
        {
            var selectedDate = date ?? DateTime.UtcNow;
            var startOfDay = selectedDate.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var vehicles = await _context.Vehicles
                .Where(v => v.Status == "Active")
                .ToListAsync();

            var bookings = await _context.VehicleBookings
                .Where(b => b.StartTime <= endOfDay && b.EndTime >= startOfDay && b.Status != "Cancelled")
                .ToListAsync();

            ViewBag.SelectedDate = selectedDate;
            ViewBag.Vehicles = vehicles;
            ViewBag.Bookings = bookings;
            ViewBag.CurrentVehicleId = vehicleId;

            return View();
        }

        // GET: VehicleBookings/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var bookings = await _context.VehicleBookings
                .Include(b => b.Vehicle)
                .Include(b => b.Driver)
                .Where(b => b.RequestedBy == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        // API: Check vehicle availability
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int vehicleId, DateTime startTime, DateTime endTime)
        {
            var conflicts = await _context.VehicleBookings
                .Where(b => b.VehicleId == vehicleId &&
                    b.Status != "Cancelled" &&
                    b.Status != "Rejected" &&
                    b.StartTime < endTime &&
                    b.EndTime > startTime)
                .Select(b => new { b.StartTime, b.EndTime, b.BookingReference, b.Status })
                .ToListAsync();

            return Json(new
            {
                IsAvailable = !conflicts.Any(),
                Conflicts = conflicts
            });
        }

        private async Task<string> GenerateBookingReference()
        {
            var prefix = "BK";
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var lastBooking = await _context.VehicleBookings
                .Where(b => b.BookingReference.StartsWith($"{prefix}-{date}"))
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastBooking != null)
            {
                var parts = lastBooking.BookingReference.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{prefix}-{date}-{sequence:D4}";
        }

        private bool BookingExists(int id)
        {
            return _context.VehicleBookings.Any(e => e.Id == id);
        }
    }
}

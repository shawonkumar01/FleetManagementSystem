using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace FleetManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class DataExportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataExportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DataExport
        public IActionResult Index()
        {
            return View();
        }

        // GET: DataExport/Vehicles
        public async Task<IActionResult> Vehicles(string format = "excel")
        {
            var vehicles = await _context.Vehicles
                .Select(v => new
                {
                    v.Id,
                    v.Make,
                    v.Model,
                    v.Year,
                    v.LicensePlate,
                    v.Status,
                    v.Mileage,
                    v.VIN,
                    v.FuelType,
                    v.PurchaseDate,
                    v.LastMaintenanceDate
                })
                .ToListAsync();

            var fileName = $"Vehicles_{DateTime.Now:yyyyMMdd_HHmmss}";

            return format.ToLower() switch
            {
                "csv" => ExportCsv(vehicles, fileName),
                "json" => ExportJson(vehicles, fileName),
                _ => ExportExcel(vehicles, fileName)
            };
        }

        // GET: DataExport/Drivers
        public async Task<IActionResult> Drivers(string format = "excel")
        {
            var drivers = await _context.Drivers
                .Select(d => new
                {
                    d.Id,
                    FullName = d.FirstName + " " + d.LastName,
                    d.LicenseNumber,
                    d.LicenseExpiry,
                    d.Phone,
                    d.Status
                })
                .ToListAsync();

            var fileName = $"Drivers_{DateTime.Now:yyyyMMdd_HHmmss}";

            return format.ToLower() switch
            {
                "csv" => ExportCsv(drivers, fileName),
                "json" => ExportJson(drivers, fileName),
                _ => ExportExcel(drivers, fileName)
            };
        }

        // GET: DataExport/Trips
        public async Task<IActionResult> Trips(DateTime? fromDate, DateTime? toDate, string format = "excel")
        {
            var query = _context.Trips.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(t => t.StartTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.StartTime <= toDate.Value);

            var trips = await query
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Select(t => new
                {
                    t.Id,
                    Vehicle = t.Vehicle.Make + " " + t.Vehicle.Model + " (" + t.Vehicle.LicensePlate + ")",
                    Driver = t.Driver.FirstName + " " + t.Driver.LastName,
                    t.Origin,
                    t.Destination,
                    t.StartTime,
                    t.EndTime,
                    t.DistanceKm,
                    t.Status
                })
                .ToListAsync();

            var fileName = $"Trips_{DateTime.Now:yyyyMMdd_HHmmss}";

            return format.ToLower() switch
            {
                "csv" => ExportCsv(trips, fileName),
                "json" => ExportJson(trips, fileName),
                _ => ExportExcel(trips, fileName)
            };
        }

        // GET: DataExport/Maintenance
        public async Task<IActionResult> Maintenance(DateTime? fromDate, DateTime? toDate, string format = "excel")
        {
            var query = _context.MaintenanceRecords.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(m => m.ServiceDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(m => m.ServiceDate <= toDate.Value);

            var records = await query
                .Include(m => m.Vehicle)
                .Select(m => new
                {
                    m.Id,
                    Vehicle = m.Vehicle.Make + " " + m.Vehicle.Model + " (" + m.Vehicle.LicensePlate + ")",
                    m.ServiceType,
                    m.ServiceDate,
                    m.NextServiceDate,
                    m.Cost,
                    m.Status,
                    m.Notes
                })
                .ToListAsync();

            var fileName = $"Maintenance_{DateTime.Now:yyyyMMdd_HHmmss}";

            return format.ToLower() switch
            {
                "csv" => ExportCsv(records, fileName),
                "json" => ExportJson(records, fileName),
                _ => ExportExcel(records, fileName)
            };
        }

        // GET: DataExport/FuelRecords
        public async Task<IActionResult> FuelRecords(DateTime? fromDate, DateTime? toDate, string format = "excel")
        {
            var query = _context.FuelRecords.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(f => f.FuelDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(f => f.FuelDate <= toDate.Value);

            var records = await query
                .Include(f => f.Vehicle)
                .Select(f => new
                {
                    f.Id,
                    Vehicle = f.Vehicle.Make + " " + f.Vehicle.Model + " (" + f.Vehicle.LicensePlate + ")",
                    f.FuelDate,
                    f.LitersFilled,
                    f.PricePerLiter,
                    f.TotalCost,
                    f.OdometerReading,
                    f.FilledBy,
                    f.StationName
                })
                .ToListAsync();

            var fileName = $"FuelRecords_{DateTime.Now:yyyyMMdd_HHmmss}";

            return format.ToLower() switch
            {
                "csv" => ExportCsv(records, fileName),
                "json" => ExportJson(records, fileName),
                _ => ExportExcel(records, fileName)
            };
        }

        // GET: DataExport/Expenses
        public async Task<IActionResult> Expenses(DateTime? fromDate, DateTime? toDate, string? category, string format = "excel")
        {
            var query = _context.Expenses.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= toDate.Value);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(e => e.Category == category);

            var expenses = await query
                .Include(e => e.Vehicle)
                .Select(e => new
                {
                    e.Id,
                    e.ExpenseDate,
                    e.Category,
                    e.Description,
                    e.Amount,
                    e.Currency,
                    e.Status,
                    e.PaymentMethod,
                    Vehicle = e.Vehicle != null ? e.Vehicle.Make + " " + e.Vehicle.Model : "N/A",
                    e.ApprovedBy,
                    e.ApprovedAt
                })
                .ToListAsync();

            var fileName = $"Expenses_{DateTime.Now:yyyyMMdd_HHmmss}";

            return format.ToLower() switch
            {
                "csv" => ExportCsv(expenses, fileName),
                "json" => ExportJson(expenses, fileName),
                _ => ExportExcel(expenses, fileName)
            };
        }

        // GET: DataExport/Bookings
        public async Task<IActionResult> Bookings(DateTime? fromDate, DateTime? toDate, string? status, string format = "excel")
        {
            var query = _context.VehicleBookings.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(b => b.StartTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(b => b.StartTime <= toDate.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(b => b.Status == status);

            var bookings = await query
                .Include(b => b.Vehicle)
                .Include(b => b.Driver)
                .Select(b => new
                {
                    b.BookingReference,
                    Vehicle = b.Vehicle.Make + " " + b.Vehicle.Model + " (" + b.Vehicle.LicensePlate + ")",
                    Driver = b.Driver != null ? b.Driver.FirstName + " " + b.Driver.LastName : "Unassigned",
                    b.Purpose,
                    b.PickupLocation,
                    b.Destination,
                    b.StartTime,
                    b.EndTime,
                    b.Status,
                    b.Priority,
                    b.RequesterName
                })
                .ToListAsync();

            var fileName = $"Bookings_{DateTime.Now:yyyyMMdd_HHmmss}";

            return format.ToLower() switch
            {
                "csv" => ExportCsv(bookings, fileName),
                "json" => ExportJson(bookings, fileName),
                _ => ExportExcel(bookings, fileName)
            };
        }

        // GET: DataExport/Documents
        public async Task<IActionResult> Documents(string? category, string format = "excel")
        {
            var query = _context.Documents.AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(d => d.Category == category);

            var documents = await query
                .Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.Category,
                    d.DocumentType,
                    d.FileName,
                    d.FileSize,
                    d.UploadedBy,
                    d.UploadedAt,
                    d.ExpiryDate,
                    d.Status
                })
                .ToListAsync();

            var fileName = $"Documents_{DateTime.Now:yyyyMMdd_HHmmss}";

            return format.ToLower() switch
            {
                "csv" => ExportCsv(documents, fileName),
                "json" => ExportJson(documents, fileName),
                _ => ExportExcel(documents, fileName)
            };
        }

        // GET: DataExport/FullReport
        public async Task<IActionResult> FullReport(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var to = toDate ?? DateTime.UtcNow;

            var report = new
            {
                ReportPeriod = new { From = from, To = to, GeneratedAt = DateTime.UtcNow },
                Summary = new
                {
                    TotalVehicles = await _context.Vehicles.CountAsync(),
                    TotalDrivers = await _context.Drivers.CountAsync(),
                    TotalTrips = await _context.Trips.CountAsync(t => t.StartTime >= from && t.StartTime <= to),
                    TotalFuelCost = await _context.FuelRecords.Where(f => f.FuelDate >= from && f.FuelDate <= to).SumAsync(f => f.TotalCost),
                    TotalMaintenanceCost = await _context.MaintenanceRecords.Where(m => m.ServiceDate >= from && m.ServiceDate <= to).SumAsync(m => m.Cost),
                    TotalExpenses = await _context.Expenses.Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to).SumAsync(e => e.Amount),
                    TotalBookings = await _context.VehicleBookings.CountAsync(b => b.StartTime >= from && b.StartTime <= to)
                },
                Vehicles = await _context.Vehicles.ToListAsync(),
                Drivers = await _context.Drivers.ToListAsync(),
                Trips = await _context.Trips.Where(t => t.StartTime >= from && t.StartTime <= to).ToListAsync(),
                FuelRecords = await _context.FuelRecords.Where(f => f.FuelDate >= from && f.FuelDate <= to).ToListAsync(),
                Maintenance = await _context.MaintenanceRecords.Where(m => m.ServiceDate >= from && m.ServiceDate <= to).ToListAsync(),
                Expenses = await _context.Expenses.Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to).ToListAsync(),
                Bookings = await _context.VehicleBookings.Where(b => b.StartTime >= from && b.StartTime <= to).ToListAsync()
            };

            var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(json);

            return File(bytes, "application/json", $"FullReport_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        }

        private IActionResult ExportCsv<T>(List<T> data, string fileName)
        {
            var csv = new StringBuilder();

            // Headers
            var properties = typeof(T).GetProperties();
            csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            // Data rows
            foreach (var item in data)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item);
                    if (value == null) return "";
                    var stringValue = value.ToString();
                    // Escape quotes and wrap in quotes if contains comma
                    if (stringValue.Contains(",") || stringValue.Contains("\""))
                    {
                        stringValue = "\"" + stringValue.Replace("\"", "\"\"") + "\"";
                    }
                    return stringValue;
                });
                csv.AppendLine(string.Join(",", values));
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"{fileName}.csv");
        }

        private IActionResult ExportJson<T>(List<T> data, string fileName)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", $"{fileName}.json");
        }

        private IActionResult ExportExcel<T>(List<T> data, string fileName)
        {
            // For now, export as CSV with Excel MIME type
            // In production, you'd use a library like EPPlus or ClosedXML
            var csv = new StringBuilder();

            var properties = typeof(T).GetProperties();
            csv.AppendLine(string.Join("\t", properties.Select(p => p.Name)));

            foreach (var item in data)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item);
                    return value?.ToString() ?? "";
                });
                csv.AppendLine(string.Join("\t", values));
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "application/vnd.ms-excel", $"{fileName}.xls");
        }
    }
}

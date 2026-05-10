using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FleetManagementSystem.Controllers
{
    [Authorize]
    public class DriverPerformanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DriverPerformanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DriverPerformance
        public async Task<IActionResult> Index(int? driverId, string? period, string? grade)
        {
            var query = _context.DriverPerformances
                .Include(p => p.Driver)
                .AsQueryable();

            // Apply filters
            if (driverId.HasValue)
                query = query.Where(p => p.DriverId == driverId);

            if (!string.IsNullOrEmpty(period))
            {
                var now = DateTime.UtcNow;
                query = period switch
                {
                    "current" => query.Where(p => p.EvaluationPeriodStart.Month == now.Month && p.EvaluationPeriodStart.Year == now.Year),
                    "last" => query.Where(p => p.EvaluationPeriodStart.Month == now.AddMonths(-1).Month && p.EvaluationPeriodStart.Year == now.AddMonths(-1).Year),
                    "quarter" => query.Where(p => p.EvaluationPeriodStart >= now.AddMonths(-3)),
                    "year" => query.Where(p => p.EvaluationPeriodStart >= now.AddYears(-1)),
                    _ => query
                };
            }

            if (!string.IsNullOrEmpty(grade))
                query = query.Where(p => p.Grade == grade);

            var performances = await query
                .OrderByDescending(p => p.EvaluationPeriodStart)
                .ToListAsync();

            // Statistics
            ViewBag.TopPerformers = await _context.DriverPerformances
                .Where(p => p.IsTopPerformer)
                .Include(p => p.Driver)
                .OrderByDescending(p => p.OverallScore)
                .Take(5)
                .ToListAsync();

            ViewBag.NeedsImprovement = await _context.DriverPerformances
                .Where(p => p.NeedsImprovement)
                .Include(p => p.Driver)
                .OrderBy(p => p.OverallScore)
                .Take(5)
                .ToListAsync();

            ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
            ViewBag.CurrentDriverId = driverId;
            ViewBag.CurrentPeriod = period;
            ViewBag.CurrentGrade = grade;

            return View(performances);
        }

        // GET: DriverPerformance/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var now = DateTime.UtcNow;
            var currentMonth = new DateTime(now.Year, now.Month, 1);

            // Overall fleet statistics
            var allPerformances = await _context.DriverPerformances.ToListAsync();
            var currentPeriod = await _context.DriverPerformances
                .Where(p => p.EvaluationPeriodStart == currentMonth)
                .ToListAsync();

            ViewBag.TotalEvaluations = allPerformances.Count;
            ViewBag.AverageFleetScore = allPerformances.Any() ? allPerformances.Average(p => p.OverallScore) : 0;
            ViewBag.TopPerformersCount = allPerformances.Count(p => p.IsTopPerformer);
            ViewBag.NeedsImprovementCount = allPerformances.Count(p => p.NeedsImprovement);

            // Grade distribution
            ViewBag.GradeDistribution = allPerformances
                .GroupBy(p => p.Grade ?? "N/A")
                .Select(g => new { Grade = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            // Category averages
            ViewBag.SafetyAverage = allPerformances.Any() ? allPerformances.Average(p => p.SafetyScore) : 0;
            ViewBag.PunctualityAverage = allPerformances.Any() ? allPerformances.Average(p => p.PunctualityScore) : 0;
            ViewBag.FuelAverage = allPerformances.Any() ? allPerformances.Average(p => p.FuelEfficiencyScore) : 0;
            ViewBag.VehicleAverage = allPerformances.Any() ? allPerformances.Average(p => p.VehicleConditionScore) : 0;
            ViewBag.ServiceAverage = allPerformances.Any() ? allPerformances.Average(p => p.CustomerServiceScore) : 0;

            // Recent incidents
            ViewBag.RecentIncidents = await _context.DriverIncidents
                .Include(i => i.Driver)
                .OrderByDescending(i => i.IncidentDate)
                .Take(10)
                .ToListAsync();

            // Driver leaderboard
            ViewBag.Leaderboard = await _context.Drivers
                .Where(d => d.Status == "Active")
                .Select(d => new
                {
                    Driver = d,
                    LatestPerformance = _context.DriverPerformances
                        .Where(p => p.DriverId == d.Id)
                        .OrderByDescending(p => p.EvaluationPeriodStart)
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.LatestPerformance != null ? x.LatestPerformance.OverallScore : 0)
                .Take(10)
                .ToListAsync();

            return View();
        }

        // GET: DriverPerformance/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var performance = await _context.DriverPerformances
                .Include(p => p.Driver)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (performance == null) return NotFound();

            // Get driver's incident history for the evaluation period
            ViewBag.Incidents = await _context.DriverIncidents
                .Where(i => i.DriverId == performance.DriverId &&
                    i.IncidentDate >= performance.EvaluationPeriodStart &&
                    i.IncidentDate <= performance.EvaluationPeriodEnd)
                .OrderByDescending(i => i.IncidentDate)
                .ToListAsync();

            // Get driver's performance history
            ViewBag.PerformanceHistory = await _context.DriverPerformances
                .Where(p => p.DriverId == performance.DriverId && p.Id != id)
                .OrderByDescending(p => p.EvaluationPeriodStart)
                .Take(6)
                .ToListAsync();

            return View(performance);
        }

        // GET: DriverPerformance/Evaluate/5
        public async Task<IActionResult> Evaluate(int? driverId)
        {
            if (driverId == null)
            {
                ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
                return View("SelectDriver");
            }

            var driver = await _context.Drivers.FindAsync(driverId);
            if (driver == null) return NotFound();

            // Calculate metrics for the last month
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var trips = await _context.Trips
                .Where(t => t.DriverId == driverId &&
                    t.StartTime >= startOfMonth &&
                    t.StartTime <= endOfMonth)
                .ToListAsync();

            var incidents = await _context.DriverIncidents
                .Where(i => i.DriverId == driverId &&
                    i.IncidentDate >= startOfMonth &&
                    i.IncidentDate <= endOfMonth)
                .ToListAsync();

            var fuelRecords = await _context.FuelRecords
                .Where(f => f.FuelDate >= startOfMonth &&
                    f.FuelDate <= endOfMonth)
                .ToListAsync();

            // Auto-calculate preliminary scores
            var performance = new DriverPerformance
            {
                DriverId = driverId.Value,
                Driver = driver,
                EvaluationPeriodStart = startOfMonth,
                EvaluationPeriodEnd = endOfMonth,
                TotalTrips = trips.Count,
                TotalDistanceKm = (decimal)trips.Sum(t => t.DistanceKm),
                AccidentsCount = incidents.Count(i => i.IncidentType == "Accident"),
                TrafficViolationsCount = incidents.Count(i => i.IncidentType == "Violation"),
                ComplaintsCount = incidents.Count(i => i.IncidentType == "Complaint"),
                ComplimentsCount = incidents.Count(i => i.IncidentType == "Compliment"),
                LateArrivalsCount = trips.Count(t => t.Status == "Delayed"),
                TotalFuelCost = fuelRecords.Sum(f => f.TotalCost)
            };

            // Auto-calculate scores based on metrics
            performance.SafetyScore = CalculateSafetyScore(performance);
            performance.PunctualityScore = CalculatePunctualityScore(performance);
            performance.FuelEfficiencyScore = CalculateFuelScore(performance, fuelRecords);
            performance.ComplianceScore = 10 - Math.Min(performance.TrafficViolationsCount, 5);

            ViewBag.Incidents = incidents;
            ViewBag.Trips = trips;

            return View(performance);
        }

        // POST: DriverPerformance/Evaluate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Evaluate(DriverPerformance performance)
        {
            if (ModelState.IsValid)
            {
                performance.RecalculateOverallScore();
                performance.Grade = performance.CalculateGrade();
                performance.IsTopPerformer = performance.OverallScore >= 9.0m;
                performance.NeedsImprovement = performance.OverallScore < 5.0m;
                performance.EvaluatedBy = User.Identity?.Name;
                performance.EvaluatedAt = DateTime.UtcNow;
                performance.Status = "Submitted";

                _context.Add(performance);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Driver performance evaluation submitted successfully!";
                return RedirectToAction(nameof(Index));
            }

            performance.Driver = await _context.Drivers.FindAsync(performance.DriverId);
            return View(performance);
        }

        // GET: DriverPerformance/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var performance = await _context.DriverPerformances
                .Include(p => p.Driver)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (performance == null) return NotFound();

            return View(performance);
        }

        // POST: DriverPerformance/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DriverPerformance performance)
        {
            if (id != performance.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    performance.RecalculateOverallScore();
                    performance.Grade = performance.CalculateGrade();
                    performance.IsTopPerformer = performance.OverallScore >= 9.0m;
                    performance.NeedsImprovement = performance.OverallScore < 5.0m;

                    _context.Update(performance);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Performance evaluation updated!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PerformanceExists(performance.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(performance);
        }

        // GET: DriverPerformance/RecordIncident
        public async Task<IActionResult> RecordIncident(int? driverId)
        {
            ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
            
            var incident = new DriverIncident();
            if (driverId.HasValue)
                incident.DriverId = driverId.Value;

            return View(incident);
        }

        // POST: DriverPerformance/RecordIncident
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordIncident(DriverIncident incident)
        {
            if (ModelState.IsValid)
            {
                incident.ReportedBy = User.Identity?.Name;
                _context.Add(incident);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Incident recorded successfully!";
                return RedirectToAction(nameof(IncidentList));
            }

            ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
            return View(incident);
        }

        // GET: DriverPerformance/IncidentList
        public async Task<IActionResult> IncidentList(string? type, string? severity, int? driverId)
        {
            var query = _context.DriverIncidents
                .Include(i => i.Driver)
                .AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(i => i.IncidentType == type);

            if (!string.IsNullOrEmpty(severity))
                query = query.Where(i => i.Severity == severity);

            if (driverId.HasValue)
                query = query.Where(i => i.DriverId == driverId);

            var incidents = await query
                .OrderByDescending(i => i.IncidentDate)
                .ToListAsync();

            ViewBag.Drivers = await _context.Drivers.ToListAsync();
            ViewBag.CurrentType = type;
            ViewBag.CurrentSeverity = severity;
            ViewBag.CurrentDriverId = driverId;

            return View(incidents);
        }

        // POST: DriverPerformance/ResolveIncident/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ResolveIncident(int id, string resolution)
        {
            var incident = await _context.DriverIncidents.FindAsync(id);
            if (incident == null) return NotFound();

            incident.Status = "Resolved";
            incident.Resolution = resolution;
            incident.ResolvedAt = DateTime.UtcNow;
            incident.ResolvedBy = User.Identity?.Name;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Incident resolved!";

            return RedirectToAction(nameof(IncidentList));
        }

        // GET: DriverPerformance/DriverReport/5
        public async Task<IActionResult> DriverReport(int? driverId)
        {
            if (driverId == null)
            {
                ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
                return View("SelectDriverForReport");
            }

            var driver = await _context.Drivers.FindAsync(driverId);
            if (driver == null) return NotFound();

            var performances = await _context.DriverPerformances
                .Where(p => p.DriverId == driverId)
                .OrderByDescending(p => p.EvaluationPeriodStart)
                .ToListAsync();

            var incidents = await _context.DriverIncidents
                .Where(i => i.DriverId == driverId)
                .OrderByDescending(i => i.IncidentDate)
                .ToListAsync();

            ViewBag.Driver = driver;
            ViewBag.Performances = performances;
            ViewBag.Incidents = incidents;
            ViewBag.AverageScore = performances.Any() ? performances.Average(p => p.OverallScore) : 0;
            ViewBag.TotalTrips = performances.Sum(p => p.TotalTrips);
            ViewBag.TotalAccidents = incidents.Count(i => i.IncidentType == "Accident");
            ViewBag.TotalViolations = incidents.Count(i => i.IncidentType == "Violation");

            return View();
        }

        // GET: DriverPerformance/Comparison
        public async Task<IActionResult> Comparison(int[]? driverIds)
        {
            if (driverIds == null || driverIds.Length < 2)
            {
                ViewBag.Drivers = await _context.Drivers.Where(d => d.Status == "Active").ToListAsync();
                return View("SelectDriversForComparison");
            }

            var performances = await _context.DriverPerformances
                .Include(p => p.Driver)
                .Where(p => driverIds.Contains(p.DriverId))
                .OrderByDescending(p => p.EvaluationPeriodStart)
                .ToListAsync();

            ViewBag.SelectedDriverIds = driverIds;

            return View(performances);
        }

        private int CalculateSafetyScore(DriverPerformance performance)
        {
            if (performance.AccidentsCount == 0) return 10;
            if (performance.AccidentsCount == 1) return 7;
            if (performance.AccidentsCount == 2) return 4;
            return 1;
        }

        private int CalculatePunctualityScore(DriverPerformance performance)
        {
            if (performance.TotalTrips == 0) return 5;
            var lateRate = (double)performance.LateArrivalsCount / performance.TotalTrips;
            if (lateRate == 0) return 10;
            if (lateRate <= 0.05) return 8;
            if (lateRate <= 0.10) return 6;
            if (lateRate <= 0.20) return 4;
            return 2;
        }

        private int CalculateFuelScore(DriverPerformance performance, List<FuelRecord> fuelRecords)
        {
            // This would ideally compare against vehicle-specific benchmarks
            // For now, simple heuristic
            if (!fuelRecords.Any()) return 5;
            var avgConsumption = (double)performance.TotalDistanceKm / fuelRecords.Sum(f => f.LitersFilled);
            if (avgConsumption >= 15) return 10; // Excellent
            if (avgConsumption >= 12) return 8;
            if (avgConsumption >= 10) return 6;
            if (avgConsumption >= 8) return 4;
            return 2;
        }

        private bool PerformanceExists(int id)
        {
            return _context.DriverPerformances.Any(e => e.Id == id);
        }
    }
}

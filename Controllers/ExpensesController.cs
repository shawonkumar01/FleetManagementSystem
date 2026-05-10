using FleetManagementSystem.Data;
using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FleetManagementSystem.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Expenses
        public async Task<IActionResult> Index(string? status, string? category, int? vehicleId, DateTime? from, DateTime? to)
        {
            var query = _context.Expenses
                .Include(e => e.Vehicle)
                .Include(e => e.Budget)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status == status);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(e => e.Category == category);

            if (vehicleId.HasValue)
                query = query.Where(e => e.VehicleId == vehicleId);

            if (from.HasValue)
                query = query.Where(e => e.ExpenseDate >= from.Value);

            if (to.HasValue)
                query = query.Where(e => e.ExpenseDate <= to.Value);

            var expenses = await query
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();

            // Calculate summary statistics
            ViewBag.TotalExpenses = expenses.Sum(e => e.Amount);
            ViewBag.PendingAmount = expenses.Where(e => e.Status == "Pending").Sum(e => e.Amount);
            ViewBag.ApprovedAmount = expenses.Where(e => e.Status == "Approved" || e.Status == "Reimbursed").Sum(e => e.Amount);
            ViewBag.TotalCount = expenses.Count;

            // Get filter options
            ViewBag.Vehicles = await _context.Vehicles.ToListAsync();
            ViewBag.Categories = await _context.Expenses.Select(e => e.Category).Distinct().ToListAsync();

            // Apply filters for display
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentVehicleId = vehicleId;
            ViewBag.FromDate = from;
            ViewBag.ToDate = to;

            return View(expenses);
        }

        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses
                .Include(e => e.Vehicle)
                .Include(e => e.Budget)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null) return NotFound();

            return View(expense);
        }

        // GET: Expenses/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
            ViewBag.Budgets = await _context.Budgets.Where(b => b.IsActive && b.EndDate >= DateTime.UtcNow).ToListAsync();

            return View();
        }

        // POST: Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense, IFormFile? receiptFile)
        {
            if (ModelState.IsValid)
            {
                // Handle receipt upload
                if (receiptFile != null && receiptFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "receipts");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + receiptFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await receiptFile.CopyToAsync(fileStream);
                    }

                    expense.ReceiptPath = "/uploads/receipts/" + uniqueFileName;
                    expense.HasReceipt = true;
                }

                _context.Add(expense);

                // Update budget spent amount
                if (expense.BudgetId.HasValue)
                {
                    var budget = await _context.Budgets.FindAsync(expense.BudgetId.Value);
                    if (budget != null)
                    {
                        budget.SpentAmount += expense.Amount;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Expense created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
            ViewBag.Budgets = await _context.Budgets.Where(b => b.IsActive && b.EndDate >= DateTime.UtcNow).ToListAsync();
            return View(expense);
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
            ViewBag.Budgets = await _context.Budgets.Where(b => b.IsActive && b.EndDate >= DateTime.UtcNow).ToListAsync();

            return View(expense);
        }

        // POST: Expenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expense expense, IFormFile? receiptFile)
        {
            if (id != expense.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingExpense = await _context.Expenses.FindAsync(id);
                    if (existingExpense == null) return NotFound();

                    // Handle receipt upload
                    if (receiptFile != null && receiptFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "receipts");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + receiptFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await receiptFile.CopyToAsync(fileStream);
                        }

                        expense.ReceiptPath = "/uploads/receipts/" + uniqueFileName;
                        expense.HasReceipt = true;
                    }
                    else
                    {
                        expense.ReceiptPath = existingExpense.ReceiptPath;
                        expense.HasReceipt = existingExpense.HasReceipt;
                    }

                    // Update budget if changed
                    if (existingExpense.BudgetId != expense.BudgetId)
                    {
                        // Remove from old budget
                        if (existingExpense.BudgetId.HasValue)
                        {
                            var oldBudget = await _context.Budgets.FindAsync(existingExpense.BudgetId.Value);
                            if (oldBudget != null)
                                oldBudget.SpentAmount -= existingExpense.Amount;
                        }

                        // Add to new budget
                        if (expense.BudgetId.HasValue)
                        {
                            var newBudget = await _context.Budgets.FindAsync(expense.BudgetId.Value);
                            if (newBudget != null)
                                newBudget.SpentAmount += expense.Amount;
                        }
                    }
                    else if (existingExpense.Amount != expense.Amount && expense.BudgetId.HasValue)
                    {
                        // Update budget amount
                        var budget = await _context.Budgets.FindAsync(expense.BudgetId.Value);
                        if (budget != null)
                        {
                            budget.SpentAmount = budget.SpentAmount - existingExpense.Amount + expense.Amount;
                        }
                    }

                    _context.Entry(existingExpense).CurrentValues.SetValues(expense);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Expense updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Vehicles = await _context.Vehicles.Where(v => v.Status == "Active").ToListAsync();
            ViewBag.Budgets = await _context.Budgets.Where(b => b.IsActive && b.EndDate >= DateTime.UtcNow).ToListAsync();
            return View(expense);
        }

        // POST: Expenses/Approve/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Approve(int id, string? comments)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            expense.Status = "Approved";
            expense.ApprovedAt = DateTime.UtcNow;
            expense.ApprovedBy = User.Identity?.Name;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Expense approved successfully!";

            return RedirectToAction(nameof(Index));
        }

        // POST: Expenses/Reject/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Reject(int id, string? comments)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            expense.Status = "Rejected";
            expense.ApprovedAt = DateTime.UtcNow;
            expense.ApprovedBy = User.Identity?.Name;
            expense.Notes = comments;

            // Remove from budget
            if (expense.BudgetId.HasValue)
            {
                var budget = await _context.Budgets.FindAsync(expense.BudgetId.Value);
                if (budget != null)
                    budget.SpentAmount -= expense.Amount;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Expense rejected!";

            return RedirectToAction(nameof(Index));
        }

        // POST: Expenses/Reimburse/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Reimburse(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            expense.Status = "Reimbursed";

            await _context.SaveChangesAsync();
            TempData["Success"] = "Expense marked as reimbursed!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses
                .Include(e => e.Vehicle)
                .Include(e => e.Budget)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null) return NotFound();

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                // Remove from budget
                if (expense.BudgetId.HasValue)
                {
                    var budget = await _context.Budgets.FindAsync(expense.BudgetId.Value);
                    if (budget != null)
                        budget.SpentAmount -= expense.Amount;
                }

                // Delete receipt file
                if (!string.IsNullOrEmpty(expense.ReceiptPath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", expense.ReceiptPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Expense deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Expenses/Report
        public async Task<IActionResult> Report(DateTime? from, DateTime? to, string? groupBy)
        {
            var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = to ?? DateTime.UtcNow;

            var expenses = await _context.Expenses
                .Include(e => e.Vehicle)
                .Where(e => e.ExpenseDate >= fromDate && e.ExpenseDate <= toDate)
                .ToListAsync();

            // Category breakdown
            var categoryData = expenses
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Amount = g.Sum(e => e.Amount), Count = g.Count() })
                .OrderByDescending(g => g.Amount)
                .ToList();

            // Vehicle breakdown
            var vehicleData = expenses
                .GroupBy(e => e.Vehicle.Make + " " + e.Vehicle.Model)
                .Select(g => new { Vehicle = g.Key, Amount = g.Sum(e => e.Amount), Count = g.Count() })
                .OrderByDescending(g => g.Amount)
                .ToList();

            // Monthly trend
            var monthlyData = expenses
                .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Amount = g.Sum(e => e.Amount)
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToList();

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.TotalAmount = expenses.Sum(e => e.Amount);
            ViewBag.CategoryData = categoryData;
            ViewBag.VehicleData = vehicleData;
            ViewBag.MonthlyData = monthlyData;

            return View(expenses);
        }

        // GET: Expenses/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            var expenses = await _context.Expenses
                .Include(e => e.Vehicle)
                .ToListAsync();

            // Dashboard statistics
            ViewBag.TotalExpenses = expenses.Sum(e => e.Amount);
            ViewBag.CurrentMonthExpenses = expenses
                .Where(e => e.ExpenseDate.Month == currentMonth && e.ExpenseDate.Year == currentYear)
                .Sum(e => e.Amount);
            ViewBag.PendingApprovals = expenses.Count(e => e.Status == "Pending");
            ViewBag.PendingAmount = expenses.Where(e => e.Status == "Pending").Sum(e => e.Amount);

            // Top spending categories
            ViewBag.TopCategories = expenses
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Amount = g.Sum(e => e.Amount) })
                .OrderByDescending(g => g.Amount)
                .Take(5)
                .ToList();

            // Recent expenses
            ViewBag.RecentExpenses = expenses
                .OrderByDescending(e => e.ExpenseDate)
                .Take(10)
                .ToList();

            // Budget status
            var budgets = await _context.Budgets
                .Where(b => b.IsActive)
                .ToListAsync();

            ViewBag.Budgets = budgets;
            ViewBag.OverBudgetCount = budgets.Count(b => b.SpentAmount > b.TotalAmount);

            return View();
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }
    }
}

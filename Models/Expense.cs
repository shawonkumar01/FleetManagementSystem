using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // Fuel, Maintenance, Insurance, Toll, Parking, etc.

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime ExpenseDate { get; set; }

        [Required]
        [Range(0, 999999999)]
        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "BDT";

        [MaxLength(50)]
        public string? PaymentMethod { get; set; } // Cash, Card, Bank Transfer, Mobile Banking

        [MaxLength(100)]
        public string? VendorName { get; set; }

        [MaxLength(500)]
        public string? ReceiptNumber { get; set; }

        public bool HasReceipt { get; set; } = false;

        [MaxLength(500)]
        public string? ReceiptPath { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Reimbursed

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(100)]
        public string? ApprovedBy { get; set; }

        // Budget tracking
        public int? BudgetId { get; set; }
        public Budget? Budget { get; set; }

        // Cost center
        [MaxLength(50)]
        public string? CostCenter { get; set; }

        [MaxLength(50)]
        public string? Department { get; set; }
    }

    public class Budget
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public decimal TotalAmount { get; set; }

        public decimal SpentAmount { get; set; } = 0;

        public decimal RemainingAmount => TotalAmount - SpentAmount;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string? CostCenter { get; set; }

        [MaxLength(50)]
        public string? Department { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }

    public class ExpenseApproval
    {
        public int Id { get; set; }

        [Required]
        public int ExpenseId { get; set; }
        public Expense Expense { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string ApproverId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ApproverName { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [MaxLength(500)]
        public string? Comments { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public int ApprovalLevel { get; set; } = 1;
    }
}

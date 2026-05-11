using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManagementSystem.Models
{
    public class VehicleBooking
    {
        public int Id { get; set; }

        // Reference Number
        [Required]
        [MaxLength(20)]
        public string BookingReference { get; set; } = string.Empty; // AUTO-GENERATED: BK-YYYYMMDD-XXXX

        // Vehicle
        [Required]
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        // Requester
        [Required]
        [MaxLength(100)]
        public string RequestedBy { get; set; } = string.Empty; // User ID or name

        [MaxLength(100)]
        public string? RequesterName { get; set; }

        [MaxLength(100)]
        public string? RequesterDepartment { get; set; }

        [MaxLength(20)]
        public string? RequesterPhone { get; set; }

        // Driver (optional - can be assigned later)
        public int? DriverId { get; set; }
        public Driver? Driver { get; set; }

        // Trip Details
        [Required]
        [MaxLength(100)]
        public string Purpose { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? TripDescription { get; set; }

        [Required]
        [MaxLength(100)]
        public string PickupLocation { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Destination { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Waypoints { get; set; } // JSON array or comma-separated

        // Schedule
        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }

        // Estimated metrics
        public decimal? EstimatedDistanceKm { get; set; }
        public decimal? EstimatedDurationHours { get; set; }

        // Actual metrics
        public decimal? ActualDistanceKm { get; set; }
        public decimal? ActualDurationHours { get; set; }

        // Passengers
        public int PassengerCount { get; set; } = 1;

        [MaxLength(500)]
        public string? PassengerNames { get; set; }

        // Priority
        [MaxLength(20)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

        // Status
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Confirmed, InProgress, Completed, Cancelled

        // Approval Workflow
        [MaxLength(100)]
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? ApprovalNotes { get; set; }

        // Rejection
        [MaxLength(100)]
        public string? RejectedBy { get; set; }

        public DateTime? RejectedAt { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        // Cancellation
        [MaxLength(100)]
        public string? CancelledBy { get; set; }

        public DateTime? CancelledAt { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        // Booking Type
        [MaxLength(20)]
        public string BookingType { get; set; } = "OneTime"; // OneTime, Recurring

        public int? RecurringBookingId { get; set; } // Link to parent recurring booking

        [MaxLength(50)]
        public string? RecurrencePattern { get; set; } // Daily, Weekly, Monthly

        // Requirements
        public bool RequiresLuggageSpace { get; set; } = false;
        public bool RequiresAirConditioning { get; set; } = false;
        public bool RequiresWheelchairAccess { get; set; } = false;
        public bool RequiresChildSeat { get; set; } = false;

        [MaxLength(500)]
        public string? SpecialRequirements { get; set; }

        // Costs
        [Column(TypeName = "decimal(12,2)")]
        public decimal? EstimatedCost { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? ActualCost { get; set; }

        // Trip Link
        public int? TripId { get; set; }
        public Trip? Trip { get; set; }

        // Check-in / Check-out
        public DateTime? CheckedOutAt { get; set; }
        [MaxLength(100)]
        public string? CheckedOutBy { get; set; }

        public DateTime? CheckedInAt { get; set; }
        [MaxLength(100)]
        public string? CheckedInBy { get; set; }

        public int? StartOdometer { get; set; }
        public int? EndOdometer { get; set; }

        // Vehicle condition at checkout/checkin
        [MaxLength(500)]
        public string? CheckoutCondition { get; set; }

        [MaxLength(500)]
        public string? CheckinCondition { get; set; }

        // Tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }
        [MaxLength(100)]
        public string? LastModifiedBy { get; set; }

        // Reminders sent
        public bool ReminderSent { get; set; } = false;
        public DateTime? ReminderSentAt { get; set; }

        // Confirmation sent
        public bool ConfirmationSent { get; set; } = false;
        public DateTime? ConfirmationSentAt { get; set; }

        // Feedback
        [MaxLength(1000)]
        public string? Feedback { get; set; }

        public int? Rating { get; set; } // 1-5 stars
    }

    public class BookingConflict
    {
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        [Required]
        public DateTime ConflictStart { get; set; }

        [Required]
        public DateTime ConflictEnd { get; set; }

        public int? ExistingBookingId { get; set; }
        public VehicleBooking? ExistingBooking { get; set; }

        public int? NewBookingId { get; set; }
        public VehicleBooking? NewBooking { get; set; }

        [MaxLength(50)]
        public string ConflictType { get; set; } = string.Empty; // Overlap, Adjacent, Gap

        [MaxLength(500)]
        public string? Resolution { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Unresolved"; // Unresolved, Resolved, Ignored

        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    }

    public class VehicleAvailability
    {
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; }

        public bool IsAvailable { get; set; } = true;

        [MaxLength(100)]
        public string? Reason { get; set; } // Maintenance, Booked, OutOfService

        public int? BookingId { get; set; }
        public VehicleBooking? Booking { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

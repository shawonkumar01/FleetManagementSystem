using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class PersonalExpense
    {
        public int Id { get; set; }

        public int PersonalVehicleId { get; set; }
        public PersonalVehicle PersonalVehicle { get; set; } = null!;

        [Required, MaxLength(50)]
        public string ExpenseType { get; set; } = "Other"; // Fuel, Maintenance, Insurance, Tax, Parking, Toll, Service, Other

        [Required]
        public DateTime ExpenseDate { get; set; }

        public decimal Amount { get; set; }

        [MaxLength(100)]
        public string? Vendor { get; set; } // Where the expense was made

        public int? OdometerReading { get; set; }

        public double? Liters { get; set; } // For fuel only

        public decimal? PricePerLiter { get; set; } // For fuel only

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class Maintenance
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        [Required, MaxLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        public DateTime ServiceDate { get; set; }

        public DateTime? NextServiceDate { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Cost { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Overdue
    }
}
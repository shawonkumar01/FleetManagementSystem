using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class Trip
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public int DriverId { get; set; }
        public Driver Driver { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Origin { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Destination { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public double DistanceKm { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Planned"; // Planned, InProgress, Completed, Cancelled
    }
}
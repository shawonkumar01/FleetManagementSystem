using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class GPSTracking
    {
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        public decimal? Altitude { get; set; } // in meters

        [Range(0, 360)]
        public decimal? Heading { get; set; } // direction in degrees

        [Range(0, 1000)]
        public decimal? Speed { get; set; } // km/h

        public bool IsActive { get; set; } = true;

        [MaxLength(100)]
        public string? Address { get; set; } // reverse geocoded address

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        public ICollection<GeofenceAlert> GeofenceAlerts { get; set; } = new List<GeofenceAlert>();
    }
}

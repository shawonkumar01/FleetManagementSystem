using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class Geofence
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // "Circle", "Polygon", "Rectangle"

        // For circular geofences
        public decimal? CenterLatitude { get; set; }
        public decimal? CenterLongitude { get; set; }
        public decimal? Radius { get; set; } // in meters

        // For polygon/rectangle geofences (JSON coordinates)
        [MaxLength(2000)]
        public string? Coordinates { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(20)]
        public string? AlertType { get; set; } // "Enter", "Exit", "Both"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public ICollection<GeofenceAlert> GeofenceAlerts { get; set; } = new List<GeofenceAlert>();
    }

    public class GeofenceAlert
    {
        public int Id { get; set; }

        [Required]
        public int GeofenceId { get; set; }
        public Geofence Geofence { get; set; } = null!;

        [Required]
        public int GPSTrackingId { get; set; }
        public GPSTracking GPSTracking { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string AlertType { get; set; } = string.Empty; // "Enter", "Exit"

        public DateTime AlertTime { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Message { get; set; }

        public bool IsRead { get; set; } = false;
    }
}

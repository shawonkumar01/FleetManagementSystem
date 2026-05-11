using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required, MaxLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, InMaintenance, Retired

        public int Mileage { get; set; }

        [MaxLength(50)]
        public string? VIN { get; set; }

        [MaxLength(20)]
        public string? FuelType { get; set; } = "Diesel";

        public DateTime? PurchaseDate { get; set; }

        public DateTime? LastMaintenanceDate { get; set; }

        // Navigation properties
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<Maintenance> MaintenanceRecords { get; set; } = new List<Maintenance>();
        public ICollection<FuelRecord> FuelRecords { get; set; } = new List<FuelRecord>();
        public ICollection<GPSTracking> GPSTrackings { get; set; } = new List<GPSTracking>();
    }
}
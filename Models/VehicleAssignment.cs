using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class VehicleAssignment
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public int DriverId { get; set; }
        public Driver Driver { get; set; } = null!;

        [Required]
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Ended

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

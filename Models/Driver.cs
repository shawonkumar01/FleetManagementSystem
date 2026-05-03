using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class Driver
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string LicenseNumber { get; set; } = string.Empty;

        public DateTime LicenseExpiry { get; set; }

        [Phone, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Inactive

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
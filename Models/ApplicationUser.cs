using Microsoft.AspNetCore.Identity;

namespace FleetManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Optional: Link to Driver for driver role users
        public int? DriverId { get; set; }
        public Driver? Driver { get; set; }
    }
}

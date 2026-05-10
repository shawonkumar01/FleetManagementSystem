using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class PersonalVehicle
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty; // e.g., "My Honda Bike"

        [Required, MaxLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        [MaxLength(20)]
        public string? LicensePlate { get; set; }

        [MaxLength(17)]
        public string? VIN { get; set; }

        public int CurrentOdometer { get; set; }

        public DateTime PurchaseDate { get; set; }

        public decimal PurchasePrice { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<PersonalVehicleDocument> Documents { get; set; } = new List<PersonalVehicleDocument>();
        public ICollection<PersonalExpense> Expenses { get; set; } = new List<PersonalExpense>();
    }
}

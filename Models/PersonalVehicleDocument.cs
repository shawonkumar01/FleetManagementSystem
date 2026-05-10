using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class PersonalVehicleDocument
    {
        public int Id { get; set; }

        public int PersonalVehicleId { get; set; }
        public PersonalVehicle PersonalVehicle { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty; // e.g., "Insurance Policy"

        [MaxLength(50)]
        public string DocumentType { get; set; } = "Other"; // Insurance, Registration, Service, Other

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(200)]
        public string? OriginalFileName { get; set; }

        public long? FileSize { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

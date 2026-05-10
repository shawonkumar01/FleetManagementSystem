using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class Alert
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Type { get; set; } = "Info"; // Info, Warning, Danger, Success

        [MaxLength(20)]
        public string Category { get; set; } = "General"; // License, Maintenance, Trip, Fuel

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public bool IsRead { get; set; } = false;

        public int? RelatedId { get; set; } // ID of related entity (Driver, Vehicle, etc.)

        [MaxLength(20)]
        public string? RelatedType { get; set; } // Type of related entity
    }
}

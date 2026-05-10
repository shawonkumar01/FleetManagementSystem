using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // Vehicle Registration, Insurance, License, Maintenance Record, etc.

        [Required]
        [MaxLength(50)]
        public string DocumentType { get; set; } = string.Empty; // PDF, Image, Word, Excel

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        public long FileSize { get; set; } // in bytes

        [MaxLength(10)]
        public string? FileExtension { get; set; }

        // Related entities (optional - can be linked to vehicles, drivers, trips, etc.)
        public int? VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public int? DriverId { get; set; }
        public Driver? Driver { get; set; }

        // Document metadata
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        
        [MaxLength(100)]
        public string? DocumentNumber { get; set; } // License number, registration number, etc.
        
        [MaxLength(100)]
        public string? IssuingAuthority { get; set; }

        // Status and tracking
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Expired, Archived

        public bool IsConfidential { get; set; } = false;

        public int Version { get; set; } = 1;

        public int? ParentDocumentId { get; set; } // For version tracking
        public Document? ParentDocument { get; set; }

        public ICollection<Document> Versions { get; set; } = new List<Document>();

        // Access control
        [MaxLength(100)]
        public string? UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? LastModifiedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        // Tags for better searchability
        [MaxLength(500)]
        public string? Tags { get; set; } // comma-separated tags

        // Notifications
        public bool ExpiryNotificationSent { get; set; } = false;
        public DateTime? NotificationSentAt { get; set; }

        // Audit
        public int DownloadCount { get; set; } = 0;
        public DateTime? LastDownloadedAt { get; set; }
    }

    public class DocumentCategory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? IconClass { get; set; } = "fas fa-file";

        public bool RequiresExpiryDate { get; set; } = false;
        public int? ExpiryWarningDays { get; set; } = 30; // Days before expiry to send warning

        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }

    public class DocumentAccessLog
    {
        public int Id { get; set; }

        [Required]
        public int DocumentId { get; set; }
        public Document Document { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? UserName { get; set; }

        [Required]
        [MaxLength(20)]
        public string Action { get; set; } = string.Empty; // View, Download, Upload, Delete, Update

        public DateTime ActionTime { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

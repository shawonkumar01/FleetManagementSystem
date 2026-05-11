using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, View, Login, Logout, Export

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty; // Vehicle, Driver, Trip, etc.

        [MaxLength(50)]
        public string? EntityId { get; set; } // ID of the affected entity

        [MaxLength(100)]
        public string? EntityName { get; set; } // Human-readable name

        // User Information
        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? UserName { get; set; }

        [MaxLength(100)]
        public string? UserRole { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        // Change Details
        [MaxLength(4000)]
        public string? OldValues { get; set; } // JSON of previous values

        [MaxLength(4000)]
        public string? NewValues { get; set; } // JSON of new values

        [MaxLength(4000)]
        public string? ChangedFields { get; set; } // Comma-separated list of changed fields

        // Request Details
        [MaxLength(10)]
        public string? HttpMethod { get; set; } // GET, POST, PUT, DELETE

        [MaxLength(2000)]
        public string? RequestPath { get; set; }

        [MaxLength(2000)]
        public string? QueryString { get; set; }

        // Result
        [MaxLength(20)]
        public string Status { get; set; } = "Success"; // Success, Failed, Denied

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        public int? ResponseStatusCode { get; set; }

        // Duration
        public long? DurationMs { get; set; } // How long the action took

        // Session Info
        [MaxLength(100)]
        public string? SessionId { get; set; }

        [MaxLength(100)]
        public string? CorrelationId { get; set; } // For tracking related actions

        // Additional Metadata
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // For bulk operations
        public int? AffectedRecordsCount { get; set; }

        // Data sensitivity
        public bool ContainsSensitiveData { get; set; } = false;

        [MaxLength(100)]
        public string? DataClassification { get; set; } // Public, Internal, Confidential, Restricted

        // Retention
        public DateTime? RetentionExpiryDate { get; set; }

        // Archive status
        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedAt { get; set; }
    }

    public class SecurityEvent
    {
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string EventType { get; set; } = string.Empty; // LoginSuccess, LoginFailure, PasswordChange, PermissionDenied, SuspiciousActivity

        [MaxLength(100)]
        public string? UserId { get; set; }

        [MaxLength(100)]
        public string? UserName { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [MaxLength(50)]
        public string Severity { get; set; } = "Info"; // Info, Warning, Critical

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? Details { get; set; } // Additional JSON data

        public bool IsResolved { get; set; } = false;

        [MaxLength(100)]
        public string? ResolvedBy { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [MaxLength(1000)]
        public string? Resolution { get; set; }

        // For failed login tracking
        public int? FailedAttemptCount { get; set; }

        public bool IsAccountLocked { get; set; } = false;

        // Geolocation (if available)
        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        // Session info
        [MaxLength(100)]
        public string? SessionId { get; set; }
    }

    public class DataAccessLog
    {
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? UserName { get; set; }

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? EntityId { get; set; }

        [MaxLength(100)]
        public string? EntityName { get; set; }

        [Required]
        [MaxLength(50)]
        public string AccessType { get; set; } = string.Empty; // View, Export, Print, Share

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? Purpose { get; set; } // Why the data was accessed

        public int RecordsAccessed { get; set; } = 1;

        public bool ContainsPii { get; set; } = false; // Personally Identifiable Information

        public bool ContainsFinancialData { get; set; } = false;

        [MaxLength(1000)]
        public string? DataFieldsAccessed { get; set; } // Which fields were viewed

        // For exports/downloads
        [MaxLength(200)]
        public string? ExportFormat { get; set; }

        [MaxLength(500)]
        public string? ExportReason { get; set; }

        public bool IsAuthorized { get; set; } = true;

        [MaxLength(1000)]
        public string? AuthorizationDetails { get; set; }
    }
}

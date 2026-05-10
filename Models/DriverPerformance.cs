using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManagementSystem.Models
{
    public class DriverPerformance
    {
        public int Id { get; set; }

        [Required]
        public int DriverId { get; set; }
        public Driver Driver { get; set; } = null!;

        [Required]
        public DateTime EvaluationPeriodStart { get; set; }

        [Required]
        public DateTime EvaluationPeriodEnd { get; set; }

        // Performance Metrics (1-10 scale)
        [Range(1, 10)]
        public int SafetyScore { get; set; } = 5;

        [Range(1, 10)]
        public int PunctualityScore { get; set; } = 5;

        [Range(1, 10)]
        public int FuelEfficiencyScore { get; set; } = 5;

        [Range(1, 10)]
        public int VehicleConditionScore { get; set; } = 5;

        [Range(1, 10)]
        public int CustomerServiceScore { get; set; } = 5;

        [Range(1, 10)]
        public int ComplianceScore { get; set; } = 5; // Following traffic rules, company policies

        // Stored Overall Score (recalculated on save)
        [Column(TypeName = "decimal(3,1)")]
        public decimal OverallScore { get; set; }

        // Detailed metrics
        public int TotalTrips { get; set; }
        public decimal TotalDistanceKm { get; set; }
        public decimal TotalDrivingHours { get; set; }

        public int AccidentsCount { get; set; }
        public int TrafficViolationsCount { get; set; }
        public int ComplaintsCount { get; set; }
        public int ComplimentsCount { get; set; }

        // Fuel metrics
        public decimal AverageFuelConsumption { get; set; } // km per liter
        public decimal TotalFuelCost { get; set; }

        // Incidents
        public int LateArrivalsCount { get; set; }
        public int EarlyDeparturesCount { get; set; }
        public int MaintenanceIssuesReported { get; set; }

        // Evaluation
        [MaxLength(1000)]
        public string? EvaluatorNotes { get; set; }

        [MaxLength(1000)]
        public string? DriverComments { get; set; }

        [MaxLength(100)]
        public string? EvaluatedBy { get; set; }

        public DateTime? EvaluatedAt { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, Reviewed, Finalized

        // Grade
        [MaxLength(5)]
        public string? Grade { get; set; } // A+, A, B+, B, C, D

        // Goals for next period
        [MaxLength(1000)]
        public string? ImprovementGoals { get; set; }

        [MaxLength(1000)]
        public string? TrainingRecommendations { get; set; }

        // Recognition
        public bool IsTopPerformer { get; set; } = false;
        public bool NeedsImprovement { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public void RecalculateOverallScore()
        {
            // Weighted average - adjust weights based on importance
            var weights = new[] { 0.30m, 0.20m, 0.20m, 0.15m, 0.10m, 0.05m };
            var scores = new[] { SafetyScore, PunctualityScore, FuelEfficiencyScore, 
                                VehicleConditionScore, CustomerServiceScore, ComplianceScore };

            decimal weightedSum = 0;
            for (int i = 0; i < scores.Length; i++)
            {
                weightedSum += scores[i] * weights[i];
            }

            OverallScore = weightedSum;
        }

        public string CalculateGrade()
        {
            return OverallScore switch
            {
                >= 9.0m => "A+",
                >= 8.0m => "A",
                >= 7.0m => "B+",
                >= 6.0m => "B",
                >= 5.0m => "C",
                >= 4.0m => "D",
                _ => "F"
            };
        }
    }

    public class DriverIncident
    {
        public int Id { get; set; }

        [Required]
        public int DriverId { get; set; }
        public Driver Driver { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string IncidentType { get; set; } = string.Empty; // Accident, Violation, Complaint, Compliment

        [Required]
        public DateTime IncidentDate { get; set; }

        [MaxLength(100)]
        public string? TripId { get; set; } // Optional reference to trip

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Resolution { get; set; }

        [MaxLength(20)]
        public string Severity { get; set; } = "Low"; // Low, Medium, High, Critical

        [MaxLength(20)]
        public string Status { get; set; } = "Open"; // Open, UnderReview, Resolved, Closed

        [Range(0, 10)]
        public int ImpactOnScore { get; set; } // Negative for incidents, positive for compliments

        [MaxLength(100)]
        public string? ReportedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        [MaxLength(100)]
        public string? ResolvedBy { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class PerformanceThreshold
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string MetricType { get; set; } = string.Empty; // Safety, Punctuality, Fuel, etc.

        public decimal MinAcceptableScore { get; set; } = 5.0m;
        public decimal TargetScore { get; set; } = 8.0m;
        public decimal ExcellentScore { get; set; } = 9.0m;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

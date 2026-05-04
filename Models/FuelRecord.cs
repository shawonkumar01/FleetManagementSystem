using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystem.Models
{
    public class FuelRecord
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        [Required]
        public DateTime FuelDate { get; set; }

        [Range(0, double.MaxValue)]
        public double LitersFilled { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PricePerLiter { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalCost { get; set; }

        public int OdometerReading { get; set; }

        [MaxLength(100)]
        public string FilledBy { get; set; } = string.Empty;

        [MaxLength(100)]
        public string StationName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
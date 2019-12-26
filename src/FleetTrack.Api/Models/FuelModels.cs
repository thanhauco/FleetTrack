using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTrack.Api.Models
{
    public class RecordFuelRequest
    {
        [Range(0.01, 2000)]
        public double Liters { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Cost { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "USD";

        [Range(0, double.MaxValue)]
        public double OdometerKm { get; set; }

        /// <summary>When the fill-up happened; defaults to now. Use for receipts entered late.</summary>
        public DateTime? FilledUtc { get; set; }
    }

    public class FuelLogDto
    {
        public Guid Id { get; set; }

        public Guid VehicleId { get; set; }

        public DateTime DateUtc { get; set; }

        public double Liters { get; set; }

        public decimal Cost { get; set; }

        public string Currency { get; set; }

        public double OdometerKm { get; set; }
    }

    public class FuelSummaryDto
    {
        public Guid VehicleId { get; set; }

        public int FillCount { get; set; }

        public double TotalLiters { get; set; }

        public double? LitersPer100Km { get; set; }

        public double? KmPerLiter { get; set; }
    }
}

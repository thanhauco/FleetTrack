using System;
using System.ComponentModel.DataAnnotations;
using FleetTrack.Core.Domain;

namespace FleetTrack.Api.Models
{
    public class RecordMaintenanceRequest
    {
        public MaintenanceType Type { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Cost { get; set; }

        public string Currency { get; set; } = "USD";

        public string Notes { get; set; }
    }

    public class MaintenanceDto
    {
        public Guid Id { get; set; }

        public Guid VehicleId { get; set; }

        public string Type { get; set; }

        public DateTime PerformedUtc { get; set; }

        public double OdometerKm { get; set; }

        public decimal Cost { get; set; }

        public string Currency { get; set; }

        public string Notes { get; set; }
    }
}

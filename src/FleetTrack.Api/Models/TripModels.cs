using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTrack.Api.Models
{
    public class GeoPointModel
    {
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }
    }

    public class PlanTripRequest
    {
        public Guid VehicleId { get; set; }

        public Guid DriverId { get; set; }

        [Required]
        public GeoPointModel Origin { get; set; }

        [Required]
        public GeoPointModel Destination { get; set; }

        public DateTime PlannedStartUtc { get; set; }
    }

    public class CompleteTripRequest
    {
        [Range(0, double.MaxValue)]
        public double EndOdometerKm { get; set; }
    }

    public class TripDto
    {
        public Guid Id { get; set; }

        public Guid VehicleId { get; set; }

        public Guid DriverId { get; set; }

        public string Status { get; set; }

        public DateTime PlannedStartUtc { get; set; }

        public DateTime? StartedUtc { get; set; }

        public DateTime? CompletedUtc { get; set; }

        public double? DistanceKm { get; set; }
    }
}

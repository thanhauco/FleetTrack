using System;
using System.ComponentModel.DataAnnotations;
using FleetTrack.Core.Domain;

namespace FleetTrack.Api.Models
{
    public class VehicleDto
    {
        public Guid Id { get; set; }

        public string LicensePlate { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }

        public int Year { get; set; }

        public string FuelType { get; set; }

        public string Category { get; set; }

        public string Status { get; set; }

        public double OdometerKm { get; set; }

        public DateTime? LastServiceUtc { get; set; }
    }

    public class CreateVehicleRequest
    {
        [Required]
        [StringLength(16)]
        public string LicensePlate { get; set; }

        [Required]
        public string Make { get; set; }

        [Required]
        public string Model { get; set; }

        [Range(1980, 2100)]
        public int Year { get; set; }

        public FuelType FuelType { get; set; }

        public VehicleCategory Category { get; set; } = VehicleCategory.Car;

        [Range(0, double.MaxValue)]
        public double OdometerKm { get; set; }
    }

    public class PagedResponse<T>
    {
        public System.Collections.Generic.IReadOnlyList<T> Items { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }
    }
}

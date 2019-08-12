using System;
using System.Collections.Generic;
using System.Linq;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Core.Services
{
    public class VehicleUtilization
    {
        public Guid VehicleId { get; set; }

        public string LicensePlate { get; set; }

        public int TripCount { get; set; }

        public double DistanceKm { get; set; }

        public double DrivingHours { get; set; }

        public double UtilizationPercent { get; set; }
    }

    public class UtilizationReportService
    {
        private readonly ITripRepository _trips;
        private readonly IVehicleRepository _vehicles;

        public UtilizationReportService(ITripRepository trips, IVehicleRepository vehicles)
        {
            _trips = Guard.NotNull(trips, nameof(trips));
            _vehicles = Guard.NotNull(vehicles, nameof(vehicles));
        }

        public IReadOnlyList<VehicleUtilization> Build(DateRange range)
        {
            var completed = _trips.FindByStatus(TripStatus.Completed)
                .Where(t => t.StartedUtc.HasValue && range.Contains(t.StartedUtc.Value))
                .ToLookup(t => t.VehicleId);

            var rows = new List<VehicleUtilization>();
            foreach (var vehicle in _vehicles.GetAll())
            {
                var trips = completed[vehicle.Id].ToList();
                var hours = trips.Sum(t => t.Duration.HasValue ? t.Duration.Value.TotalHours : 0);

                rows.Add(new VehicleUtilization
                {
                    VehicleId = vehicle.Id,
                    LicensePlate = vehicle.LicensePlate,
                    TripCount = trips.Count,
                    DistanceKm = trips.Sum(t => t.DistanceKm ?? 0),
                    DrivingHours = hours,
                    UtilizationPercent = range.TotalHours > 0 ? Math.Round(hours / range.TotalHours * 100.0, 2) : 0
                });
            }

            return rows.OrderByDescending(r => r.UtilizationPercent).ThenBy(r => r.LicensePlate).ToList();
        }
    }
}

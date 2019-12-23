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

    /// <summary>
    /// Driving time of completed trips as a share of the reporting period.
    /// </summary>
    /// <remarks>
    /// Only the part of a trip that falls inside the range is counted, so a trip
    /// crossing either boundary contributes partially and utilization can never
    /// exceed 100 % per vehicle. Distance is pro-rated by the same time fraction,
    /// which assumes a roughly constant average speed over the trip; odometer
    /// readings are only taken at the start and end, so nothing finer is known.
    /// </remarks>
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
                .Where(t => t.StartedUtc.HasValue && t.CompletedUtc.HasValue
                            && range.Intersects(t.StartedUtc.Value, t.CompletedUtc.Value))
                .ToLookup(t => t.VehicleId);

            var rows = new List<VehicleUtilization>();
            foreach (var vehicle in _vehicles.GetAll())
            {
                var trips = completed[vehicle.Id].ToList();
                var hours = 0.0;
                var distance = 0.0;

                foreach (var trip in trips)
                {
                    var overlap = range.Overlap(trip.StartedUtc.Value, trip.CompletedUtc.Value);
                    hours += overlap.TotalHours;
                    distance += (trip.DistanceKm ?? 0) * FractionInRange(trip, overlap);
                }

                rows.Add(new VehicleUtilization
                {
                    VehicleId = vehicle.Id,
                    LicensePlate = vehicle.LicensePlate,
                    TripCount = trips.Count,
                    DistanceKm = Math.Round(distance, 1),
                    DrivingHours = Math.Round(hours, 2),
                    UtilizationPercent = range.TotalHours > 0 ? Math.Round(hours / range.TotalHours * 100.0, 2) : 0
                });
            }

            return rows.OrderByDescending(r => r.UtilizationPercent).ThenBy(r => r.LicensePlate).ToList();
        }

        private static double FractionInRange(Trip trip, TimeSpan overlap)
        {
            var duration = trip.Duration.Value;

            // A zero-length trip has no time to pro-rate; it is either inside or not.
            return duration > TimeSpan.Zero ? overlap.TotalHours / duration.TotalHours : 1.0;
        }
    }
}

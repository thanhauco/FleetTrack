using System;
using FleetTrack.Core.Common;

namespace FleetTrack.Core.Domain
{
    public class Trip : Entity
    {
        public Trip(Guid vehicleId, Guid driverId, GeoPoint origin, GeoPoint destination, DateTime plannedStartUtc)
        {
            if (vehicleId == Guid.Empty)
            {
                throw new ArgumentException("Vehicle id is required.", nameof(vehicleId));
            }

            if (driverId == Guid.Empty)
            {
                throw new ArgumentException("Driver id is required.", nameof(driverId));
            }

            VehicleId = vehicleId;
            DriverId = driverId;
            Origin = origin;
            Destination = destination;
            PlannedStartUtc = plannedStartUtc;
            Status = TripStatus.Planned;
        }

        public Guid VehicleId { get; private set; }

        public Guid DriverId { get; private set; }

        public GeoPoint Origin { get; private set; }

        public GeoPoint Destination { get; private set; }

        public DateTime PlannedStartUtc { get; private set; }

        public DateTime? StartedUtc { get; private set; }

        public DateTime? CompletedUtc { get; private set; }

        public double StartOdometerKm { get; private set; }

        public double? EndOdometerKm { get; private set; }

        public TripStatus Status { get; private set; }

        public double? DistanceKm => EndOdometerKm.HasValue ? EndOdometerKm.Value - StartOdometerKm : (double?)null;

        public TimeSpan? Duration => StartedUtc.HasValue && CompletedUtc.HasValue
            ? CompletedUtc.Value - StartedUtc.Value
            : (TimeSpan?)null;

        public void Start(DateTime nowUtc, double odometerKm)
        {
            if (Status != TripStatus.Planned)
            {
                throw new DomainException($"Only planned trips can be started (current: {Status}).");
            }

            StartedUtc = nowUtc;
            StartOdometerKm = Guard.NotNegative(odometerKm, nameof(odometerKm));
            Status = TripStatus.InProgress;
        }

        public void Complete(DateTime nowUtc, double endOdometerKm)
        {
            if (Status != TripStatus.InProgress)
            {
                throw new DomainException($"Only trips in progress can be completed (current: {Status}).");
            }

            if (endOdometerKm < StartOdometerKm)
            {
                throw new DomainException("End odometer cannot be lower than the start odometer.");
            }

            CompletedUtc = nowUtc;
            EndOdometerKm = endOdometerKm;
            Status = TripStatus.Completed;
        }

        public void Cancel()
        {
            if (Status == TripStatus.Completed || Status == TripStatus.Cancelled)
            {
                throw new DomainException($"A {Status} trip cannot be cancelled.");
            }

            Status = TripStatus.Cancelled;
        }
    }
}

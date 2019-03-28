using System;

namespace FleetTrack.Core.Events
{
    public class TripCompletedEvent : IDomainEvent
    {
        public TripCompletedEvent(Guid tripId, Guid vehicleId, Guid driverId, double distanceKm, DateTime occurredUtc)
        {
            TripId = tripId;
            VehicleId = vehicleId;
            DriverId = driverId;
            DistanceKm = distanceKm;
            OccurredUtc = occurredUtc;
        }

        public Guid TripId { get; }

        public Guid VehicleId { get; }

        public Guid DriverId { get; }

        public double DistanceKm { get; }

        public DateTime OccurredUtc { get; }
    }
}

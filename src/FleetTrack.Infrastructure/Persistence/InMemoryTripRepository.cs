using System;
using System.Collections.Generic;
using System.Linq;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Infrastructure.Persistence
{
    public class InMemoryTripRepository : InMemoryRepository<Trip>, ITripRepository
    {
        public IReadOnlyList<Trip> FindByVehicle(Guid vehicleId)
        {
            return Items.Where(t => t.VehicleId == vehicleId).OrderBy(t => t.PlannedStartUtc).ToList();
        }

        public IReadOnlyList<Trip> FindByDriver(Guid driverId)
        {
            return Items.Where(t => t.DriverId == driverId).OrderBy(t => t.PlannedStartUtc).ToList();
        }

        public IReadOnlyList<Trip> FindByStatus(TripStatus status)
        {
            return Items.Where(t => t.Status == status).OrderBy(t => t.PlannedStartUtc).ToList();
        }
    }
}

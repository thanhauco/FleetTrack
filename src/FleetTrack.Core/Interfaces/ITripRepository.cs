using System;
using System.Collections.Generic;
using FleetTrack.Core.Domain;

namespace FleetTrack.Core.Interfaces
{
    public interface ITripRepository : IRepository<Trip>
    {
        IReadOnlyList<Trip> FindByVehicle(Guid vehicleId);

        IReadOnlyList<Trip> FindByDriver(Guid driverId);

        IReadOnlyList<Trip> FindByStatus(TripStatus status);
    }
}

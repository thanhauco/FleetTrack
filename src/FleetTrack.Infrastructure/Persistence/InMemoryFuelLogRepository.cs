using System;
using System.Collections.Generic;
using System.Linq;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Infrastructure.Persistence
{
    public class InMemoryFuelLogRepository : InMemoryRepository<FuelLog>, IFuelLogRepository
    {
        public IReadOnlyList<FuelLog> FindByVehicle(Guid vehicleId)
        {
            return Items.Where(l => l.VehicleId == vehicleId).OrderBy(l => l.DateUtc).ToList();
        }
    }
}

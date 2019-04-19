using System;
using System.Collections.Generic;
using System.Linq;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Infrastructure.Persistence
{
    public class InMemoryMaintenanceRepository : InMemoryRepository<MaintenanceRecord>, IMaintenanceRepository
    {
        public IReadOnlyList<MaintenanceRecord> FindByVehicle(Guid vehicleId)
        {
            return Items.Where(r => r.VehicleId == vehicleId).OrderBy(r => r.PerformedUtc).ToList();
        }
    }
}

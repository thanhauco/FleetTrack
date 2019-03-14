using System;
using System.Collections.Generic;
using FleetTrack.Core.Domain;

namespace FleetTrack.Core.Interfaces
{
    public interface IMaintenanceRepository : IRepository<MaintenanceRecord>
    {
        IReadOnlyList<MaintenanceRecord> FindByVehicle(Guid vehicleId);
    }
}

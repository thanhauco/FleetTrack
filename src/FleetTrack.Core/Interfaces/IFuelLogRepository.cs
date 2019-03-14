using System;
using System.Collections.Generic;
using FleetTrack.Core.Domain;

namespace FleetTrack.Core.Interfaces
{
    public interface IFuelLogRepository : IRepository<FuelLog>
    {
        IReadOnlyList<FuelLog> FindByVehicle(Guid vehicleId);
    }
}

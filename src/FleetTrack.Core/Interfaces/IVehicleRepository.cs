using System.Collections.Generic;
using FleetTrack.Core.Domain;

namespace FleetTrack.Core.Interfaces
{
    public interface IVehicleRepository : IRepository<Vehicle>
    {
        Vehicle FindByPlate(string licensePlate);

        IReadOnlyList<Vehicle> FindByStatus(VehicleStatus status);
    }
}

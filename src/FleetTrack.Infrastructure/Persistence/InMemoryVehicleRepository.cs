using System;
using System.Collections.Generic;
using System.Linq;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Infrastructure.Persistence
{
    public class InMemoryVehicleRepository : InMemoryRepository<Vehicle>, IVehicleRepository
    {
        public Vehicle FindByPlate(string licensePlate)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                return null;
            }

            var normalized = licensePlate.Trim();
            return Items.FirstOrDefault(v => string.Equals(v.LicensePlate, normalized, StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<Vehicle> FindByStatus(VehicleStatus status)
        {
            return Items.Where(v => v.Status == status).ToList();
        }
    }
}

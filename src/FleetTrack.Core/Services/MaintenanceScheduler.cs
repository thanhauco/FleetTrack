using System;
using FleetTrack.Core.Domain;

namespace FleetTrack.Core.Services
{
    public class MaintenanceScheduler
    {
        private const double IntervalKm = 10000;

        public bool IsDue(Vehicle vehicle, DateTime nowUtc)
        {
            if (vehicle.Status == VehicleStatus.Retired)
            {
                return false;
            }

            return vehicle.OdometerKm - vehicle.LastServiceOdometerKm >= IntervalKm;
        }
    }
}

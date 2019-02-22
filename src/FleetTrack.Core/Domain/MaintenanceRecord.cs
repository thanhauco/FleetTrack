using System;
using FleetTrack.Core.Common;

namespace FleetTrack.Core.Domain
{
    public class MaintenanceRecord : Entity
    {
        public MaintenanceRecord(Guid vehicleId, MaintenanceType type, DateTime performedUtc, double odometerKm, Money cost, string notes = null)
        {
            if (vehicleId == Guid.Empty)
            {
                throw new ArgumentException("Vehicle id is required.", nameof(vehicleId));
            }

            VehicleId = vehicleId;
            Type = type;
            PerformedUtc = performedUtc;
            OdometerKm = Guard.NotNegative(odometerKm, nameof(odometerKm));
            Cost = cost;
            Notes = notes;
        }

        public Guid VehicleId { get; private set; }

        public MaintenanceType Type { get; private set; }

        public DateTime PerformedUtc { get; private set; }

        public double OdometerKm { get; private set; }

        public Money Cost { get; private set; }

        public string Notes { get; private set; }
    }
}

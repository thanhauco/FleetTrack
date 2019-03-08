using System;
using FleetTrack.Core.Common;

namespace FleetTrack.Core.Domain
{
    public class FuelLog : Entity
    {
        public FuelLog(Guid vehicleId, DateTime dateUtc, double liters, Money cost, double odometerKm)
        {
            if (vehicleId == Guid.Empty)
            {
                throw new ArgumentException("Vehicle id is required.", nameof(vehicleId));
            }

            VehicleId = vehicleId;
            DateUtc = dateUtc;
            Liters = Guard.Positive(liters, nameof(liters));
            Cost = cost;
            OdometerKm = Guard.NotNegative(odometerKm, nameof(odometerKm));
        }

        public Guid VehicleId { get; private set; }

        public DateTime DateUtc { get; private set; }

        public double Liters { get; private set; }

        public Money Cost { get; private set; }

        public double OdometerKm { get; private set; }
    }
}

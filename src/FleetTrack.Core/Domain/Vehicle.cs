using System;
using FleetTrack.Core.Common;

namespace FleetTrack.Core.Domain
{
    public class Vehicle : Entity
    {
        // Repositories hand out shared instances, so every status transition is a
        // check-then-set that must be atomic: two callers must never both see
        // "Available" and both assign the vehicle.
        private readonly object _sync = new object();

        public Vehicle(
            string licensePlate,
            string make,
            string model,
            int year,
            FuelType fuelType,
            double odometerKm,
            VehicleCategory category = VehicleCategory.Car)
        {
            if (!Enum.IsDefined(typeof(VehicleCategory), category))
            {
                throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown vehicle category.");
            }

            Category = category;
            LicensePlate = Guard.NotEmpty(licensePlate, nameof(licensePlate)).Trim().ToUpperInvariant();
            Make = Guard.NotEmpty(make, nameof(make)).Trim();
            Model = Guard.NotEmpty(model, nameof(model)).Trim();
            Year = Guard.InRange(year, 1980, 2100, nameof(year));
            FuelType = fuelType;
            OdometerKm = Guard.NotNegative(odometerKm, nameof(odometerKm));
            Status = VehicleStatus.Available;
        }

        public string LicensePlate { get; private set; }

        public string Make { get; private set; }

        public string Model { get; private set; }

        public int Year { get; private set; }

        public FuelType FuelType { get; private set; }

        public VehicleCategory Category { get; private set; }

        public double OdometerKm { get; private set; }

        public VehicleStatus Status { get; private set; }

        public DateTime? LastServiceUtc { get; private set; }

        public double LastServiceOdometerKm { get; private set; }

        public int Age(int currentYear)
        {
            return Math.Max(0, currentYear - Year);
        }

        public void UpdateOdometer(double km)
        {
            lock (_sync)
            {
                if (km < OdometerKm)
                {
                    throw new DomainException("Odometer cannot move backwards.");
                }

                OdometerKm = km;
            }
        }

        public void AssignToTrip()
        {
            lock (_sync)
            {
                if (Status != VehicleStatus.Available)
                {
                    throw new DomainException($"Vehicle {LicensePlate} is not available.");
                }

                Status = VehicleStatus.InUse;
            }
        }

        public void ReleaseFromTrip()
        {
            lock (_sync)
            {
                if (Status == VehicleStatus.InUse)
                {
                    Status = VehicleStatus.Available;
                }
            }
        }

        public void SendToMaintenance()
        {
            lock (_sync)
            {
                if (Status == VehicleStatus.InUse || Status == VehicleStatus.Retired)
                {
                    throw new DomainException($"Vehicle {LicensePlate} cannot enter maintenance while {Status}.");
                }

                Status = VehicleStatus.InMaintenance;
            }
        }

        public void CompleteMaintenance(DateTime performedUtc)
        {
            lock (_sync)
            {
                if (Status == VehicleStatus.Retired)
                {
                    throw new DomainException("Retired vehicles cannot be serviced.");
                }

                // Completing a service sets the vehicle Available; doing that while it
                // is out on a trip would free it for a second, overlapping trip.
                if (Status == VehicleStatus.InUse)
                {
                    throw new DomainException($"Vehicle {LicensePlate} is on a trip and cannot be serviced until it returns.");
                }

                LastServiceUtc = performedUtc;
                LastServiceOdometerKm = OdometerKm;
                Status = VehicleStatus.Available;
            }
        }

        public void Retire()
        {
            lock (_sync)
            {
                if (Status == VehicleStatus.InUse)
                {
                    throw new DomainException("A vehicle in use cannot be retired.");
                }

                Status = VehicleStatus.Retired;
            }
        }
    }
}

using System;
using FleetTrack.Core.Common;

namespace FleetTrack.Core.Domain
{
    public class Vehicle : Entity
    {
        public Vehicle(string licensePlate, string make, string model, int year, FuelType fuelType, double odometerKm)
        {
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

        public double OdometerKm { get; private set; }

        public VehicleStatus Status { get; private set; }

        public void UpdateOdometer(double km)
        {
            if (km < OdometerKm)
            {
                throw new DomainException("Odometer cannot move backwards.");
            }

            OdometerKm = km;
        }

        public void AssignToTrip()
        {
            if (Status != VehicleStatus.Available)
            {
                throw new DomainException($"Vehicle {LicensePlate} is not available.");
            }

            Status = VehicleStatus.InUse;
        }

        public void ReleaseFromTrip()
        {
            if (Status == VehicleStatus.InUse)
            {
                Status = VehicleStatus.Available;
            }
        }
    }
}

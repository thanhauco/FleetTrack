using System;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Domain
{
    public class VehicleTests
    {
        [Fact]
        public void Constructor_NormalizesPlate()
        {
            var vehicle = new Vehicle(" ab-123 ", "Volvo", "FH16", 2018, FuelType.Diesel, 10);

            Assert.Equal("AB-123", vehicle.LicensePlate);
            Assert.Equal(VehicleStatus.Available, vehicle.Status);
        }

        [Fact]
        public void UpdateOdometer_CannotGoBackwards()
        {
            var vehicle = TestData.Vehicle(odometerKm: 500);

            Assert.Throws<DomainException>(() => vehicle.UpdateOdometer(499));
        }

        [Fact]
        public void AssignToTrip_RequiresAvailableVehicle()
        {
            var vehicle = TestData.Vehicle();
            vehicle.AssignToTrip();

            Assert.Equal(VehicleStatus.InUse, vehicle.Status);
            Assert.Throws<DomainException>(() => vehicle.AssignToTrip());
        }
    }
}

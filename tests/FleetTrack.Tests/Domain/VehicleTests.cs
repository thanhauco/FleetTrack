using System;
using System.Threading;
using System.Threading.Tasks;
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

        [Theory]
        [InlineData(1979)]
        [InlineData(2101)]
        public void Constructor_RejectsImplausibleYear(int year)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Vehicle("A", "Volvo", "FH16", year, FuelType.Diesel, 0));
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

        [Fact]
        public void ReleaseFromTrip_MakesVehicleAvailableAgain()
        {
            var vehicle = TestData.Vehicle();
            vehicle.AssignToTrip();
            vehicle.ReleaseFromTrip();

            Assert.Equal(VehicleStatus.Available, vehicle.Status);
        }

        [Fact]
        public void Maintenance_CycleRecordsServiceData()
        {
            var vehicle = TestData.Vehicle(odometerKm: 12000);
            vehicle.SendToMaintenance();
            Assert.Equal(VehicleStatus.InMaintenance, vehicle.Status);

            vehicle.CompleteMaintenance(TestData.Now);

            Assert.Equal(VehicleStatus.Available, vehicle.Status);
            Assert.Equal(TestData.Now, vehicle.LastServiceUtc);
            Assert.Equal(12000, vehicle.LastServiceOdometerKm);
        }

        [Fact]
        public void SendToMaintenance_Throws_WhileInUse()
        {
            var vehicle = TestData.Vehicle();
            vehicle.AssignToTrip();

            Assert.Throws<DomainException>(() => vehicle.SendToMaintenance());
        }

        [Fact]
        public void Retire_BlocksFurtherUse()
        {
            var vehicle = TestData.Vehicle();
            vehicle.Retire();

            Assert.Equal(VehicleStatus.Retired, vehicle.Status);
            Assert.Throws<DomainException>(() => vehicle.AssignToTrip());
            Assert.Throws<DomainException>(() => vehicle.CompleteMaintenance(TestData.Now));
        }

        [Fact]
        public void CompleteMaintenance_Throws_WhileInUse()
        {
            var vehicle = TestData.Vehicle();
            vehicle.AssignToTrip();

            Assert.Throws<DomainException>(() => vehicle.CompleteMaintenance(TestData.Now));
            Assert.Equal(VehicleStatus.InUse, vehicle.Status);
            Assert.Null(vehicle.LastServiceUtc);
        }

        [Fact]
        public void AssignToTrip_IsAtomicUnderContention()
        {
            var vehicle = TestData.Vehicle();
            var successes = 0;

            Parallel.For(0, 64, _ =>
            {
                try
                {
                    vehicle.AssignToTrip();
                    Interlocked.Increment(ref successes);
                }
                catch (DomainException)
                {
                }
            });

            Assert.Equal(1, successes);
        }

        [Fact]
        public void Age_IsNeverNegative()
        {
            var vehicle = TestData.Vehicle();

            Assert.Equal(1, vehicle.Age(2019));
            Assert.Equal(0, vehicle.Age(2010));
        }
    }
}

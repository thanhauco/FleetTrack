using System;
using System.Collections.Generic;
using FleetTrack.Core.Domain;
using FleetTrack.Infrastructure.Persistence;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Infrastructure
{
    public class InMemoryRepositoryTests
    {
        [Fact]
        public void Add_ThenGet_ReturnsSameInstance()
        {
            var repo = new InMemoryVehicleRepository();
            var vehicle = TestData.Vehicle();

            repo.Add(vehicle);

            Assert.Same(vehicle, repo.Get(vehicle.Id));
        }

        [Fact]
        public void Get_UnknownId_ReturnsNull()
        {
            Assert.Null(new InMemoryVehicleRepository().Get(Guid.NewGuid()));
        }

        [Fact]
        public void Add_Twice_Throws()
        {
            var repo = new InMemoryVehicleRepository();
            var vehicle = TestData.Vehicle();
            repo.Add(vehicle);

            Assert.Throws<InvalidOperationException>(() => repo.Add(vehicle));
        }

        [Fact]
        public void Update_UnknownEntity_Throws()
        {
            Assert.Throws<KeyNotFoundException>(() => new InMemoryVehicleRepository().Update(TestData.Vehicle()));
        }

        [Fact]
        public void Remove_ReportsWhetherEntityExisted()
        {
            var repo = new InMemoryVehicleRepository();
            var vehicle = TestData.Vehicle();
            repo.Add(vehicle);

            Assert.True(repo.Remove(vehicle.Id));
            Assert.False(repo.Remove(vehicle.Id));
        }

        [Fact]
        public void FindByPlate_IsCaseInsensitive()
        {
            var repo = new InMemoryVehicleRepository();
            repo.Add(TestData.Vehicle("AB-123"));

            Assert.NotNull(repo.FindByPlate(" ab-123 "));
            Assert.Null(repo.FindByPlate("ZZ-999"));
            Assert.Null(repo.FindByPlate(null));
        }

        [Fact]
        public void FindByStatus_FiltersVehicles()
        {
            var repo = new InMemoryVehicleRepository();
            var free = TestData.Vehicle("A-1");
            var busy = TestData.Vehicle("A-2");
            busy.AssignToTrip();
            repo.Add(free);
            repo.Add(busy);

            Assert.Equal(new[] { busy }, repo.FindByStatus(VehicleStatus.InUse));
        }

        [Fact]
        public void DriverRepository_FindsExpiringLicenses()
        {
            var repo = new InMemoryDriverRepository();
            repo.Add(TestData.Driver("D-1", 5));
            repo.Add(TestData.Driver("D-2", 50));

            var found = repo.FindWithExpiringLicense(TestData.Now, TestData.Now.AddDays(10));

            Assert.Single(found);
            Assert.Equal("D-1", found[0].LicenseNumber);
        }

        [Fact]
        public void TripRepository_FiltersByVehicleAndStatus()
        {
            var repo = new InMemoryTripRepository();
            var vehicleId = Guid.NewGuid();
            var trip = new Trip(vehicleId, Guid.NewGuid(), TestData.Berlin, TestData.Munich, TestData.Now);
            repo.Add(trip);
            repo.Add(new Trip(Guid.NewGuid(), Guid.NewGuid(), TestData.Berlin, TestData.Munich, TestData.Now));

            Assert.Single(repo.FindByVehicle(vehicleId));
            Assert.Equal(2, repo.FindByStatus(TripStatus.Planned).Count);
            Assert.Empty(repo.FindByStatus(TripStatus.Completed));
        }
    }
}

using System;
using System.Linq;
using FleetTrack.Core.Services;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Services
{
    public class MaintenanceSchedulerTests
    {
        private readonly MaintenanceScheduler _scheduler = new MaintenanceScheduler();

        [Fact]
        public void IsDue_WhenDistanceIntervalExceeded()
        {
            var vehicle = TestData.Vehicle(odometerKm: 10000);

            Assert.True(_scheduler.IsDue(vehicle, TestData.Now));
        }

        [Fact]
        public void IsDue_False_ForFreshVehicle()
        {
            var vehicle = TestData.Vehicle(odometerKm: 500);

            Assert.False(_scheduler.IsDue(vehicle, TestData.Now));
        }

        [Fact]
        public void IsDue_WhenTimeIntervalExceeded()
        {
            var vehicle = TestData.Vehicle(odometerKm: 500);
            vehicle.CompleteMaintenance(TestData.Now.AddDays(-181));

            Assert.True(_scheduler.IsDue(vehicle, TestData.Now));
        }

        [Fact]
        public void IsDue_False_ForRetiredVehicle()
        {
            var vehicle = TestData.Vehicle(odometerKm: 50000);
            vehicle.Retire();

            Assert.False(_scheduler.IsDue(vehicle, TestData.Now));
        }

        [Fact]
        public void IsDue_UsesCustomOptions()
        {
            var scheduler = new MaintenanceScheduler(new MaintenanceSchedulerOptions { IntervalKm = 1000 });

            Assert.True(scheduler.IsDue(TestData.Vehicle(odometerKm: 1000), TestData.Now));
        }

        [Fact]
        public void GetDue_FiltersVehicles()
        {
            var due = TestData.Vehicle("DUE-1", 20000);
            var fine = TestData.Vehicle("FINE-1", 100);

            var result = _scheduler.GetDue(new[] { due, fine }, TestData.Now);

            Assert.Single(result);
            Assert.Equal("DUE-1", result.Single().LicensePlate);
        }

        [Fact]
        public void EstimateNextServiceDate_ByDistance()
        {
            var vehicle = TestData.Vehicle(odometerKm: 5000);

            var estimate = _scheduler.EstimateNextServiceDate(vehicle, 100, TestData.Now);

            Assert.Equal(TestData.Now.AddDays(50), estimate);
        }

        [Fact]
        public void EstimateNextServiceDate_PrefersEarlierTimeInterval()
        {
            var vehicle = TestData.Vehicle(odometerKm: 100);
            vehicle.CompleteMaintenance(TestData.Now.AddDays(-170));

            var estimate = _scheduler.EstimateNextServiceDate(vehicle, 10, TestData.Now);

            Assert.Equal(TestData.Now.AddDays(10), estimate);
        }

        [Fact]
        public void EstimateNextServiceDate_NullWhenNoUsageAndNoHistory()
        {
            Assert.Null(_scheduler.EstimateNextServiceDate(TestData.Vehicle(), 0, TestData.Now));
        }
    }
}

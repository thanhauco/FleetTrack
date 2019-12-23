using System;
using System.Linq;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Services;
using FleetTrack.Infrastructure.Persistence;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Services
{
    public class UtilizationReportServiceTests
    {
        private readonly InMemoryTripRepository _trips = new InMemoryTripRepository();
        private readonly InMemoryVehicleRepository _vehicles = new InMemoryVehicleRepository();
        private readonly UtilizationReportService _service;

        public UtilizationReportServiceTests()
        {
            _service = new UtilizationReportService(_trips, _vehicles);
        }

        private void AddCompletedTrip(Vehicle vehicle, DateTime start, double hours, double km)
        {
            var trip = new Trip(vehicle.Id, Guid.NewGuid(), TestData.Berlin, TestData.Munich, start);
            trip.Start(start, 1000);
            trip.Complete(start.AddHours(hours), 1000 + km);
            _trips.Add(trip);
        }

        [Fact]
        public void Build_AggregatesTripsPerVehicle()
        {
            var busy = TestData.Vehicle("BUSY-1");
            var idle = TestData.Vehicle("IDLE-1");
            _vehicles.Add(busy);
            _vehicles.Add(idle);

            var start = TestData.Now.Date;
            AddCompletedTrip(busy, start.AddHours(8), 6, 400);
            AddCompletedTrip(busy, start.AddHours(20), 6, 350);

            var rows = _service.Build(new DateRange(start, start.AddHours(48)));

            Assert.Equal(2, rows.Count);
            Assert.Equal("BUSY-1", rows[0].LicensePlate);
            Assert.Equal(2, rows[0].TripCount);
            Assert.Equal(750, rows[0].DistanceKm);
            Assert.Equal(12, rows[0].DrivingHours);
            Assert.Equal(25.0, rows[0].UtilizationPercent);
            Assert.Equal(0, rows[1].TripCount);
        }

        [Fact]
        public void Build_IgnoresTripsOutsideRange()
        {
            var vehicle = TestData.Vehicle();
            _vehicles.Add(vehicle);
            AddCompletedTrip(vehicle, TestData.Now.AddDays(-30), 5, 100);

            var rows = _service.Build(new DateRange(TestData.Now.AddDays(-1), TestData.Now));

            Assert.Equal(0, rows[0].TripCount);
        }

        [Fact]
        public void Build_ClipsTripRunningPastEndOfRange()
        {
            var vehicle = TestData.Vehicle();
            _vehicles.Add(vehicle);
            var start = TestData.Now.Date;
            AddCompletedTrip(vehicle, start.AddHours(8), 30, 3000);

            var row = _service.Build(new DateRange(start, start.AddHours(10))).Single();

            Assert.Equal(1, row.TripCount);
            Assert.Equal(2, row.DrivingHours);
            Assert.Equal(20.0, row.UtilizationPercent);
            Assert.Equal(200, row.DistanceKm);
        }

        [Fact]
        public void Build_CountsTripStartedBeforeRange()
        {
            var vehicle = TestData.Vehicle();
            _vehicles.Add(vehicle);
            var start = TestData.Now.Date;
            AddCompletedTrip(vehicle, start.AddHours(-4), 6, 600);

            var row = _service.Build(new DateRange(start, start.AddHours(24))).Single();

            Assert.Equal(1, row.TripCount);
            Assert.Equal(2, row.DrivingHours);
            Assert.Equal(200, row.DistanceKm);
        }

        [Fact]
        public void Build_NeverExceedsOneHundredPercent()
        {
            var vehicle = TestData.Vehicle();
            _vehicles.Add(vehicle);
            var start = TestData.Now.Date;
            AddCompletedTrip(vehicle, start.AddHours(-10), 50, 4000);

            var row = _service.Build(new DateRange(start, start.AddHours(24))).Single();

            Assert.Equal(100.0, row.UtilizationPercent);
        }
    }
}

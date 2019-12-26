using System;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Events;
using FleetTrack.Core.Services;
using FleetTrack.Infrastructure.Persistence;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Services
{
    public class FuelServiceTests
    {
        private readonly InMemoryFuelLogRepository _fuel = new InMemoryFuelLogRepository();
        private readonly InMemoryVehicleRepository _vehicles = new InMemoryVehicleRepository();
        private readonly FakeClock _clock = new FakeClock(TestData.Now);
        private readonly FuelService _service;
        private readonly Vehicle _vehicle;

        public FuelServiceTests()
        {
            _service = new FuelService(_fuel, _vehicles, _clock);
            _vehicle = TestData.Vehicle(odometerKm: 5000);
            _vehicles.Add(_vehicle);
        }

        private bool Fill(double odometer, int daysAgo, double liters = 40)
        {
            return _service.RecordFill(_vehicle.Id, liters, TestData.Usd(60), odometer, TestData.Now.AddDays(-daysAgo)).IsSuccess;
        }

        [Fact]
        public void RecordFill_AddsLog_AndAdvancesVehicleOdometer()
        {
            var result = _service.RecordFill(_vehicle.Id, 45.5, TestData.Usd(70), 5400);

            Assert.True(result.IsSuccess);
            Assert.Equal(TestData.Now, result.Value.DateUtc);
            Assert.Single(_fuel.FindByVehicle(_vehicle.Id));
            Assert.Equal(5400, _vehicle.OdometerKm);
        }

        [Fact]
        public void RecordFill_LateEntry_DoesNotMoveVehicleOdometerBack()
        {
            Assert.True(Fill(4800, 10));

            Assert.Equal(5000, _vehicle.OdometerKm);
        }

        [Fact]
        public void RecordFill_AcceptsLateEntryBetweenNeighbours()
        {
            Assert.True(Fill(4000, 20));
            Assert.True(Fill(4900, 2));

            Assert.True(Fill(4500, 10));
            Assert.Equal(3, _fuel.FindByVehicle(_vehicle.Id).Count);
        }

        [Fact]
        public void RecordFill_RejectsReadingBelowEarlierFill()
        {
            Assert.True(Fill(4900, 10));

            var result = _service.RecordFill(_vehicle.Id, 40, TestData.Usd(60), 4800, TestData.Now.AddDays(-5));

            Assert.True(result.IsFailure);
            Assert.Contains("below the fill-up", result.Error);
        }

        [Fact]
        public void RecordFill_RejectsReadingAboveLaterFill()
        {
            Assert.True(Fill(4500, 2));

            var result = _service.RecordFill(_vehicle.Id, 40, TestData.Usd(60), 4600, TestData.Now.AddDays(-5));

            Assert.True(result.IsFailure);
            Assert.Contains("above the later fill-up", result.Error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void RecordFill_RejectsInvalidLitres(double liters)
        {
            Assert.True(_service.RecordFill(_vehicle.Id, liters, TestData.Usd(10), 5100).IsFailure);
        }

        [Fact]
        public void RecordFill_RejectsNegativeCost_FutureDate_AndUnknownVehicle()
        {
            Assert.True(_service.RecordFill(_vehicle.Id, 40, TestData.Usd(-1), 5100).IsFailure);
            Assert.True(_service.RecordFill(_vehicle.Id, 40, TestData.Usd(60), 5100, TestData.Now.AddHours(1)).IsFailure);
            Assert.True(_service.RecordFill(Guid.NewGuid(), 40, TestData.Usd(60), 5100).IsFailure);
            Assert.Empty(_fuel.GetAll());
        }

        [Fact]
        public void RecordFill_RejectsElectricAndRetiredVehicles()
        {
            var leaf = TestData.Vehicle("EV-1", 100, FuelType.Electric);
            var old = TestData.Vehicle("OLD-1", 100);
            old.Retire();
            _vehicles.Add(leaf);
            _vehicles.Add(old);

            Assert.Contains("electric", _service.RecordFill(leaf.Id, 40, TestData.Usd(60), 200).Error);
            Assert.Contains("retired", _service.RecordFill(old.Id, 40, TestData.Usd(60), 200).Error);
        }

        [Fact]
        public void Summarise_ComputesConsumption()
        {
            Fill(4000, 20, 50);
            Fill(4500, 10, 40);
            Fill(5000, 1, 35);

            var summary = _service.Summarise(_vehicle.Id).Value;

            Assert.Equal(3, summary.FillCount);
            Assert.Equal(125, summary.TotalLiters);
            Assert.Equal(7.5, summary.LitersPer100Km.Value, 6);
            Assert.True(_service.Summarise(Guid.NewGuid()).IsFailure);
        }

        [Fact]
        public void FillDuringTrip_GuardsAgainstMistypedEndOdometer()
        {
            var drivers = new InMemoryDriverRepository();
            var trips = new InMemoryTripRepository();
            var driver = TestData.Driver();
            drivers.Add(driver);
            var tripService = new TripService(trips, _vehicles, drivers, _clock, new InProcessEventBus());
            var trip = tripService.Plan(_vehicle.Id, driver.Id, TestData.Berlin, TestData.Munich, TestData.Now).Value;
            tripService.Start(trip.Id);

            Assert.True(_service.RecordFill(_vehicle.Id, 300, TestData.Usd(420), 5320).IsSuccess);

            Assert.True(tripService.Complete(trip.Id, 5230).IsFailure);
            Assert.Equal(TripStatus.InProgress, trips.Get(trip.Id).Status);
            Assert.True(tripService.Complete(trip.Id, 5504).IsSuccess);
        }
    }
}

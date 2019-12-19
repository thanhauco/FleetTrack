using System;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Events;
using FleetTrack.Core.Services;
using FleetTrack.Infrastructure.Persistence;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Services
{
    public class TripServiceTests
    {
        private readonly InMemoryTripRepository _trips = new InMemoryTripRepository();
        private readonly InMemoryVehicleRepository _vehicles = new InMemoryVehicleRepository();
        private readonly InMemoryDriverRepository _drivers = new InMemoryDriverRepository();
        private readonly InProcessEventBus _bus = new InProcessEventBus();
        private readonly FakeClock _clock = new FakeClock(TestData.Now);
        private readonly TripService _service;
        private readonly Vehicle _vehicle;
        private readonly Driver _driver;

        public TripServiceTests()
        {
            _service = new TripService(_trips, _vehicles, _drivers, _clock, _bus);
            _vehicle = TestData.Vehicle(odometerKm: 2000);
            _driver = TestData.Driver();
            _vehicles.Add(_vehicle);
            _drivers.Add(_driver);
        }

        private Trip PlanTrip()
        {
            return _service.Plan(_vehicle.Id, _driver.Id, TestData.Berlin, TestData.Munich, TestData.Now).Value;
        }

        [Fact]
        public void Plan_CreatesPlannedTrip()
        {
            var result = _service.Plan(_vehicle.Id, _driver.Id, TestData.Berlin, TestData.Munich, TestData.Now);

            Assert.True(result.IsSuccess);
            Assert.Equal(TripStatus.Planned, result.Value.Status);
        }

        [Fact]
        public void Plan_FailsForUnknownVehicleOrDriver()
        {
            Assert.True(_service.Plan(Guid.NewGuid(), _driver.Id, TestData.Berlin, TestData.Munich, TestData.Now).IsFailure);
            Assert.True(_service.Plan(_vehicle.Id, Guid.NewGuid(), TestData.Berlin, TestData.Munich, TestData.Now).IsFailure);
        }

        [Fact]
        public void Plan_FailsForExpiredLicence()
        {
            var expired = TestData.Driver("D-EXP", -2);
            _drivers.Add(expired);

            Assert.True(_service.Plan(_vehicle.Id, expired.Id, TestData.Berlin, TestData.Munich, TestData.Now).IsFailure);
        }

        [Fact]
        public void Plan_FailsWhenLicenceExpiresBeforePlannedStart()
        {
            var expiringSoon = TestData.Driver("D-SOON", 10);
            _drivers.Add(expiringSoon);

            var result = _service.Plan(_vehicle.Id, expiringSoon.Id, TestData.Berlin, TestData.Munich, TestData.Now.AddDays(30));

            Assert.True(result.IsFailure);
            Assert.Contains("before the planned start", result.Error);
        }

        [Fact]
        public void Plan_SucceedsWhenLicenceStillValidOnPlannedStart()
        {
            var expiringSoon = TestData.Driver("D-SOON", 10);
            _drivers.Add(expiringSoon);

            Assert.True(_service.Plan(_vehicle.Id, expiringSoon.Id, TestData.Berlin, TestData.Munich, TestData.Now.AddDays(10)).IsSuccess);
        }

        [Fact]
        public void Start_FailsWhenLicenceExpiredSincePlanning()
        {
            var expiringSoon = TestData.Driver("D-SOON", 1);
            _drivers.Add(expiringSoon);
            var trip = _service.Plan(_vehicle.Id, expiringSoon.Id, TestData.Berlin, TestData.Munich, TestData.Now).Value;
            _clock.Advance(TimeSpan.FromDays(5));

            Assert.True(_service.Start(trip.Id).IsFailure);
            Assert.Equal(VehicleStatus.Available, _vehicles.Get(_vehicle.Id).Status);
        }

        [Fact]
        public void Start_FailsWhenDriverDeactivatedSincePlanning()
        {
            var trip = PlanTrip();
            _driver.Deactivate();

            Assert.True(_service.Start(trip.Id).IsFailure);
        }

        [Fact]
        public void Plan_FailsForRetiredVehicle()
        {
            _vehicle.Retire();

            Assert.True(_service.Plan(_vehicle.Id, _driver.Id, TestData.Berlin, TestData.Munich, TestData.Now).IsFailure);
        }

        [Fact]
        public void Start_AssignsVehicleAndRecordsOdometer()
        {
            var trip = PlanTrip();

            var result = _service.Start(trip.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(TripStatus.InProgress, result.Value.Status);
            Assert.Equal(2000, result.Value.StartOdometerKm);
            Assert.Equal(VehicleStatus.InUse, _vehicles.Get(_vehicle.Id).Status);
        }

        [Fact]
        public void Start_FailsWhenVehicleAlreadyInUse()
        {
            var first = PlanTrip();
            var second = PlanTrip();
            _service.Start(first.Id);

            Assert.True(_service.Start(second.Id).IsFailure);
        }

        [Fact]
        public void Complete_UpdatesOdometerReleasesVehicleAndPublishesEvent()
        {
            TripCompletedEvent published = null;
            _bus.Subscribe<TripCompletedEvent>(e => published = e);

            var trip = PlanTrip();
            _service.Start(trip.Id);
            _clock.Advance(TimeSpan.FromHours(5));

            var result = _service.Complete(trip.Id, 2504.5);

            Assert.True(result.IsSuccess);
            Assert.Equal(504.5, result.Value.DistanceKm);
            Assert.Equal(2504.5, _vehicles.Get(_vehicle.Id).OdometerKm);
            Assert.Equal(VehicleStatus.Available, _vehicles.Get(_vehicle.Id).Status);
            Assert.NotNull(published);
            Assert.Equal(trip.Id, published.TripId);
            Assert.Equal(504.5, published.DistanceKm);
        }

        [Fact]
        public void Complete_Succeeds_EvenWhenASubscriberThrows()
        {
            _bus.Subscribe<TripCompletedEvent>(e => { throw new InvalidOperationException("notifier down"); });
            var trip = PlanTrip();
            _service.Start(trip.Id);

            var result = _service.Complete(trip.Id, 2100);

            Assert.True(result.IsSuccess);
            Assert.Equal(TripStatus.Completed, _trips.Get(trip.Id).Status);
            Assert.Equal(VehicleStatus.Available, _vehicles.Get(_vehicle.Id).Status);
        }

        [Fact]
        public void Complete_FailsWhenOdometerLowerThanStart()
        {
            var trip = PlanTrip();
            _service.Start(trip.Id);

            Assert.True(_service.Complete(trip.Id, 1500).IsFailure);
        }

        [Fact]
        public void Complete_FailsForTripThatWasNeverStarted()
        {
            var trip = PlanTrip();

            Assert.True(_service.Complete(trip.Id, 3000).IsFailure);
        }

        [Fact]
        public void Cancel_ReleasesVehicleOfRunningTrip()
        {
            var trip = PlanTrip();
            _service.Start(trip.Id);

            var result = _service.Cancel(trip.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(VehicleStatus.Available, _vehicles.Get(_vehicle.Id).Status);
            Assert.Equal(TripStatus.Cancelled, _trips.Get(trip.Id).Status);
        }

        [Fact]
        public void Cancel_FailsForUnknownTrip()
        {
            Assert.True(_service.Cancel(Guid.NewGuid()).IsFailure);
        }
    }
}

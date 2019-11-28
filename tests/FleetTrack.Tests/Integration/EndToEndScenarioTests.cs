using System;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Events;
using FleetTrack.Core.Handlers;
using FleetTrack.Core.Services;
using FleetTrack.Infrastructure.Notifications;
using FleetTrack.Infrastructure.Persistence;
using FleetTrack.Tests.Helpers;
using System.IO;
using Xunit;

namespace FleetTrack.Tests.Integration
{
    public class EndToEndScenarioTests
    {
        [Fact]
        public void FullDay_OfOperations()
        {
            var clock = new FakeClock(TestData.Now);
            var vehicles = new InMemoryVehicleRepository();
            var drivers = new InMemoryDriverRepository();
            var trips = new InMemoryTripRepository();
            var maintenance = new InMemoryMaintenanceRepository();
            var fuel = new InMemoryFuelLogRepository();
            var bus = new InProcessEventBus();
            var output = new StringWriter();
            var longTrips = new LongTripNotifier(bus, new ConsoleNotifier(output), 400);

            var vehicleService = new VehicleService(vehicles);
            var driverService = new DriverService(drivers, clock);
            var tripService = new TripService(trips, vehicles, drivers, clock, bus);
            var costs = new CostReportService(maintenance, fuel, vehicles);

            var vehicle = vehicleService.Register("E2E-1", "Scania", "R450", 2018, FuelType.Diesel, 9800).Value;
            var driver = driverService.Hire("Ava Nguyen", "D-500", LicenseClass.CE, clock.UtcNow.AddYears(2)).Value;

            var trip = tripService.Plan(vehicle.Id, driver.Id, TestData.Berlin, TestData.Munich, clock.UtcNow).Value;
            Assert.True(tripService.Start(trip.Id).IsSuccess);

            fuel.Add(new FuelLog(vehicle.Id, clock.UtcNow, 300, TestData.Usd(420), 9800));
            clock.Advance(TimeSpan.FromHours(6));
            Assert.True(tripService.Complete(trip.Id, 10320).IsSuccess);
            fuel.Add(new FuelLog(vehicle.Id, clock.UtcNow, 160, TestData.Usd(224), 10320));

            Assert.Contains("Long trip completed", output.ToString());

            var scheduler = new MaintenanceScheduler();
            Assert.True(scheduler.IsDue(vehicles.Get(vehicle.Id), clock.UtcNow));

            var report = costs.Build(vehicle.Id, TestData.Now.AddDays(-1), clock.UtcNow.AddDays(1)).Value;
            Assert.Equal(520, report.DistanceKm);
            Assert.Equal(TestData.Usd(644), report.FuelCost);

            var consumption = FuelEfficiencyCalculator.LitersPer100Km(fuel.FindByVehicle(vehicle.Id));
            Assert.Equal(160.0 / 520.0 * 100.0, consumption.Value, 6);
        }
    }
}

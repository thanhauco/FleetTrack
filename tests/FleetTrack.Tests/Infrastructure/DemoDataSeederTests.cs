using System.Linq;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Services;
using FleetTrack.Infrastructure.Persistence;
using FleetTrack.Infrastructure.Seed;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Infrastructure
{
    public class DemoDataSeederTests
    {
        [Fact]
        public void Seed_PopulatesAllRepositories()
        {
            var vehicles = new InMemoryVehicleRepository();
            var drivers = new InMemoryDriverRepository();
            var maintenance = new InMemoryMaintenanceRepository();
            var fuel = new InMemoryFuelLogRepository();

            new DemoDataSeeder(vehicles, drivers, maintenance, fuel, new FakeClock(TestData.Now)).Seed();

            Assert.Equal(4, vehicles.GetAll().Count);
            Assert.Equal(3, drivers.GetAll().Count);
            Assert.Equal(2, maintenance.GetAll().Count);
            Assert.Equal(5, fuel.GetAll().Count);
        }

        [Fact]
        public void SeededFleet_ProducesAlerts()
        {
            var vehicles = new InMemoryVehicleRepository();
            var drivers = new InMemoryDriverRepository();
            var clock = new FakeClock(TestData.Now);
            new DemoDataSeeder(vehicles, drivers, new InMemoryMaintenanceRepository(), new InMemoryFuelLogRepository(), clock).Seed();

            var alerts = new AlertService(vehicles, drivers, new MaintenanceScheduler(), clock).GetAlerts();

            Assert.Contains(alerts, a => a.Severity == AlertSeverity.Critical);
            Assert.Contains(alerts, a => a.Message.Contains("FT-1001"));
        }
    }
}

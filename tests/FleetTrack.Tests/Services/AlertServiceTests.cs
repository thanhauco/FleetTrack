using System.Linq;
using FleetTrack.Core.Services;
using FleetTrack.Infrastructure.Persistence;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Services
{
    public class AlertServiceTests
    {
        private readonly InMemoryVehicleRepository _vehicles = new InMemoryVehicleRepository();
        private readonly InMemoryDriverRepository _drivers = new InMemoryDriverRepository();
        private readonly AlertService _service;

        public AlertServiceTests()
        {
            _service = new AlertService(_vehicles, _drivers, new MaintenanceScheduler(), new FakeClock(TestData.Now));
        }

        [Fact]
        public void NoAlerts_ForHealthyFleet()
        {
            _vehicles.Add(TestData.Vehicle(odometerKm: 100));
            _drivers.Add(TestData.Driver(expiresInDays: 300));

            Assert.Empty(_service.GetAlerts());
        }

        [Fact]
        public void MaintenanceDue_RaisesWarning()
        {
            _vehicles.Add(TestData.Vehicle("OLD-1", 30000));

            var alert = Assert.Single(_service.GetAlerts());

            Assert.Equal(AlertSeverity.Warning, alert.Severity);
            Assert.Contains("OLD-1", alert.Message);
        }

        [Fact]
        public void ExpiredLicence_IsCritical_AndSortedFirst()
        {
            _vehicles.Add(TestData.Vehicle("OLD-1", 30000));
            _drivers.Add(TestData.Driver("D-1", -5));

            var alerts = _service.GetAlerts();

            Assert.Equal(2, alerts.Count);
            Assert.Equal(AlertSeverity.Critical, alerts[0].Severity);
        }

        [Fact]
        public void ExpiringSoon_IsWarning()
        {
            _drivers.Add(TestData.Driver("D-1", 12));

            var alert = _service.GetAlerts().Single();

            Assert.Equal(AlertSeverity.Warning, alert.Severity);
            Assert.Contains("12 day", alert.Message);
        }

        [Fact]
        public void InactiveDrivers_AreIgnored()
        {
            var driver = TestData.Driver("D-1", -5);
            driver.Deactivate();
            _drivers.Add(driver);

            Assert.Empty(_service.GetAlerts());
        }
    }
}

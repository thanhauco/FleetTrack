using System.Collections.Generic;
using System.Linq;
using FleetTrack.Core.Common;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Core.Services
{
    public enum AlertSeverity
    {
        Info = 0,
        Warning = 1,
        Critical = 2
    }

    public class Alert
    {
        public Alert(AlertSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public AlertSeverity Severity { get; }

        public string Message { get; }
    }

    public class AlertService
    {
        private const int LicenseWarningDays = 30;

        private readonly IVehicleRepository _vehicles;
        private readonly IDriverRepository _drivers;
        private readonly MaintenanceScheduler _scheduler;
        private readonly IClock _clock;

        public AlertService(IVehicleRepository vehicles, IDriverRepository drivers, MaintenanceScheduler scheduler, IClock clock)
        {
            _vehicles = Guard.NotNull(vehicles, nameof(vehicles));
            _drivers = Guard.NotNull(drivers, nameof(drivers));
            _scheduler = Guard.NotNull(scheduler, nameof(scheduler));
            _clock = Guard.NotNull(clock, nameof(clock));
        }

        public IReadOnlyList<Alert> GetAlerts()
        {
            var now = _clock.UtcNow;
            var alerts = new List<Alert>();

            foreach (var vehicle in _vehicles.GetAll())
            {
                if (_scheduler.IsDue(vehicle, now))
                {
                    alerts.Add(new Alert(AlertSeverity.Warning, $"Vehicle {vehicle.LicensePlate} is due for maintenance."));
                }
            }

            foreach (var driver in _drivers.GetAll().Where(d => d.IsActive))
            {
                var days = driver.DaysUntilExpiry(now);
                if (days < 0)
                {
                    alerts.Add(new Alert(AlertSeverity.Critical, $"Licence of {driver.FullName} expired {-days} day(s) ago."));
                }
                else if (days <= LicenseWarningDays)
                {
                    alerts.Add(new Alert(AlertSeverity.Warning, $"Licence of {driver.FullName} expires in {days} day(s)."));
                }
            }

            return alerts.OrderByDescending(a => a.Severity).ToList();
        }
    }
}

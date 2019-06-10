using System;
using System.Collections.Generic;
using System.Linq;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;

namespace FleetTrack.Core.Services
{
    public class MaintenanceSchedulerOptions
    {
        public double IntervalKm { get; set; } = 10000;

        public int IntervalDays { get; set; } = 180;
    }

    public class MaintenanceScheduler
    {
        private readonly MaintenanceSchedulerOptions _options;

        public MaintenanceScheduler(MaintenanceSchedulerOptions options = null)
        {
            _options = options ?? new MaintenanceSchedulerOptions();
        }

        public bool IsDue(Vehicle vehicle, DateTime nowUtc)
        {
            Guard.NotNull(vehicle, nameof(vehicle));

            if (vehicle.Status == VehicleStatus.Retired)
            {
                return false;
            }

            if (vehicle.OdometerKm - vehicle.LastServiceOdometerKm >= _options.IntervalKm)
            {
                return true;
            }

            if (vehicle.LastServiceUtc.HasValue)
            {
                return (nowUtc - vehicle.LastServiceUtc.Value).TotalDays >= _options.IntervalDays;
            }

            return false;
        }

        public IReadOnlyList<Vehicle> GetDue(IEnumerable<Vehicle> vehicles, DateTime nowUtc)
        {
            Guard.NotNull(vehicles, nameof(vehicles));
            return vehicles.Where(v => IsDue(v, nowUtc)).ToList();
        }

        /// <summary>
        /// Estimates the date of the next service from the average daily distance,
        /// whichever of the distance or time interval is reached first.
        /// </summary>
        public DateTime? EstimateNextServiceDate(Vehicle vehicle, double averageKmPerDay, DateTime nowUtc)
        {
            Guard.NotNull(vehicle, nameof(vehicle));

            if (vehicle.Status == VehicleStatus.Retired)
            {
                return null;
            }

            DateTime? byDistance = null;
            if (averageKmPerDay > 0)
            {
                var remainingKm = _options.IntervalKm - (vehicle.OdometerKm - vehicle.LastServiceOdometerKm);
                byDistance = remainingKm <= 0 ? nowUtc : nowUtc.AddDays(remainingKm / averageKmPerDay);
            }

            DateTime? byTime = vehicle.LastServiceUtc.HasValue
                ? vehicle.LastServiceUtc.Value.AddDays(_options.IntervalDays)
                : (DateTime?)null;

            if (byDistance.HasValue && byTime.HasValue)
            {
                return byDistance.Value < byTime.Value ? byDistance : byTime;
            }

            return byDistance ?? byTime;
        }
    }
}

using System;
using System.Linq;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Core.Services
{
    public class FuelSummary
    {
        public Guid VehicleId { get; set; }

        public int FillCount { get; set; }

        public double TotalLiters { get; set; }

        public double? LitersPer100Km { get; set; }

        public double? KmPerLiter { get; set; }
    }

    /// <summary>
    /// Records fill-ups and summarises consumption.
    /// </summary>
    /// <remarks>
    /// Receipts are often entered days late, so a fill-up is validated against its
    /// neighbours in time rather than against the most recent entry: its odometer
    /// reading must not be below any earlier fill-up or above any later one. A reading
    /// newer than the vehicle's own odometer advances it; an older one leaves it alone.
    /// </remarks>
    public class FuelService
    {
        private readonly object _gate = new object();
        private readonly IFuelLogRepository _fuelLogs;
        private readonly IVehicleRepository _vehicles;
        private readonly IClock _clock;

        public FuelService(IFuelLogRepository fuelLogs, IVehicleRepository vehicles, IClock clock)
        {
            _fuelLogs = Guard.NotNull(fuelLogs, nameof(fuelLogs));
            _vehicles = Guard.NotNull(vehicles, nameof(vehicles));
            _clock = Guard.NotNull(clock, nameof(clock));
        }

        public Result<FuelLog> RecordFill(Guid vehicleId, double liters, Money cost, double odometerKm, DateTime? filledUtc = null)
        {
            var now = _clock.UtcNow;
            var when = filledUtc ?? now;

            if (when > now)
            {
                return Result.Fail<FuelLog>("A fill-up cannot be dated in the future.");
            }

            if (double.IsNaN(liters) || double.IsInfinity(liters) || liters <= 0)
            {
                return Result.Fail<FuelLog>("Litres must be a positive number.");
            }

            if (double.IsNaN(odometerKm) || double.IsInfinity(odometerKm) || odometerKm < 0)
            {
                return Result.Fail<FuelLog>("Odometer must be a non-negative number.");
            }

            if (cost.Amount < 0)
            {
                return Result.Fail<FuelLog>("Cost cannot be negative.");
            }

            lock (_gate)
            {
                var vehicle = _vehicles.Get(vehicleId);
                if (vehicle == null)
                {
                    return Result.Fail<FuelLog>("Vehicle not found.");
                }

                if (vehicle.Status == VehicleStatus.Retired)
                {
                    return Result.Fail<FuelLog>($"Vehicle {vehicle.LicensePlate} is retired.");
                }

                if (vehicle.FuelType == FuelType.Electric)
                {
                    return Result.Fail<FuelLog>($"Vehicle {vehicle.LicensePlate} is electric and does not take fuel.");
                }

                var logs = _fuelLogs.FindByVehicle(vehicleId);

                var earlier = logs.Where(l => l.DateUtc <= when).OrderByDescending(l => l.OdometerKm).FirstOrDefault();
                if (earlier != null && odometerKm < earlier.OdometerKm)
                {
                    return Result.Fail<FuelLog>(
                        $"Odometer {odometerKm:F0} km is below the fill-up of {earlier.DateUtc:yyyy-MM-dd} ({earlier.OdometerKm:F0} km).");
                }

                var later = logs.Where(l => l.DateUtc > when).OrderBy(l => l.OdometerKm).FirstOrDefault();
                if (later != null && odometerKm > later.OdometerKm)
                {
                    return Result.Fail<FuelLog>(
                        $"Odometer {odometerKm:F0} km is above the later fill-up of {later.DateUtc:yyyy-MM-dd} ({later.OdometerKm:F0} km).");
                }

                var log = new FuelLog(vehicleId, when, liters, cost, odometerKm);

                if (odometerKm > vehicle.OdometerKm)
                {
                    vehicle.UpdateOdometer(odometerKm);
                    _vehicles.Update(vehicle);
                }

                _fuelLogs.Add(log);
                return Result.Ok(log);
            }
        }

        public Result<FuelSummary> Summarise(Guid vehicleId)
        {
            if (_vehicles.Get(vehicleId) == null)
            {
                return Result.Fail<FuelSummary>("Vehicle not found.");
            }

            var logs = _fuelLogs.FindByVehicle(vehicleId);
            return Result.Ok(new FuelSummary
            {
                VehicleId = vehicleId,
                FillCount = logs.Count,
                TotalLiters = logs.Sum(l => l.Liters),
                LitersPer100Km = FuelEfficiencyCalculator.LitersPer100Km(logs),
                KmPerLiter = FuelEfficiencyCalculator.KmPerLiter(logs)
            });
        }
    }
}

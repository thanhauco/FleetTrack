using System;
using System.Linq;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Core.Services
{
    public class VehicleCostReport
    {
        public Guid VehicleId { get; set; }

        public DateTime FromUtc { get; set; }

        public DateTime ToUtc { get; set; }

        public Money MaintenanceCost { get; set; }

        public Money FuelCost { get; set; }

        public Money TotalCost => MaintenanceCost + FuelCost;

        public double DistanceKm { get; set; }

        public decimal? CostPerKm => DistanceKm > 0
            ? decimal.Round(TotalCost.Amount / (decimal)DistanceKm, 4)
            : (decimal?)null;
    }

    public class CostReportService
    {
        private readonly IMaintenanceRepository _maintenance;
        private readonly IFuelLogRepository _fuelLogs;
        private readonly IVehicleRepository _vehicles;

        public CostReportService(IMaintenanceRepository maintenance, IFuelLogRepository fuelLogs, IVehicleRepository vehicles)
        {
            _maintenance = Guard.NotNull(maintenance, nameof(maintenance));
            _fuelLogs = Guard.NotNull(fuelLogs, nameof(fuelLogs));
            _vehicles = Guard.NotNull(vehicles, nameof(vehicles));
        }

        public Result<VehicleCostReport> Build(Guid vehicleId, DateTime fromUtc, DateTime toUtc, string currency = "USD")
        {
            if (_vehicles.Get(vehicleId) == null)
            {
                return Result.Fail<VehicleCostReport>("Vehicle not found.");
            }

            if (toUtc < fromUtc)
            {
                return Result.Fail<VehicleCostReport>("The end of the period cannot be before its start.");
            }

            var maintenance = _maintenance.FindByVehicle(vehicleId)
                .Where(r => r.PerformedUtc >= fromUtc && r.PerformedUtc <= toUtc)
                .ToList();
            var fuel = _fuelLogs.FindByVehicle(vehicleId)
                .Where(l => l.DateUtc >= fromUtc && l.DateUtc <= toUtc)
                .OrderBy(l => l.OdometerKm)
                .ToList();

            var distance = fuel.Count >= 2 ? fuel[fuel.Count - 1].OdometerKm - fuel[0].OdometerKm : 0;

            try
            {
                var report = new VehicleCostReport
                {
                    VehicleId = vehicleId,
                    FromUtc = fromUtc,
                    ToUtc = toUtc,
                    MaintenanceCost = maintenance.Aggregate(Money.Zero(currency), (sum, r) => sum + r.Cost),
                    FuelCost = fuel.Aggregate(Money.Zero(currency), (sum, l) => sum + l.Cost),
                    DistanceKm = distance
                };

                return Result.Ok(report);
            }
            catch (DomainException ex)
            {
                return Result.Fail<VehicleCostReport>(ex.Message);
            }
        }
    }
}

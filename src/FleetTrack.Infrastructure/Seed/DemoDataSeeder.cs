using System;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Infrastructure.Seed
{
    /// <summary>
    /// Fills the in-memory repositories with a small, deterministic data set.
    /// </summary>
    public class DemoDataSeeder
    {
        private readonly IVehicleRepository _vehicles;
        private readonly IDriverRepository _drivers;
        private readonly IMaintenanceRepository _maintenance;
        private readonly IFuelLogRepository _fuelLogs;
        private readonly IClock _clock;

        public DemoDataSeeder(
            IVehicleRepository vehicles,
            IDriverRepository drivers,
            IMaintenanceRepository maintenance,
            IFuelLogRepository fuelLogs,
            IClock clock)
        {
            _vehicles = Guard.NotNull(vehicles, nameof(vehicles));
            _drivers = Guard.NotNull(drivers, nameof(drivers));
            _maintenance = Guard.NotNull(maintenance, nameof(maintenance));
            _fuelLogs = Guard.NotNull(fuelLogs, nameof(fuelLogs));
            _clock = Guard.NotNull(clock, nameof(clock));
        }

        public void Seed()
        {
            var now = _clock.UtcNow;

            var volvo = new Vehicle("FT-1001", "Volvo", "FH16", 2017, FuelType.Diesel, 182000, VehicleCategory.ArticulatedTruck);
            var transit = new Vehicle("FT-1002", "Ford", "Transit", 2018, FuelType.Diesel, 64000, VehicleCategory.Van);
            var prius = new Vehicle("FT-1003", "Toyota", "Prius", 2019, FuelType.Hybrid, 9000);
            var leaf = new Vehicle("FT-1004", "Nissan", "Leaf", 2019, FuelType.Electric, 4200);

            foreach (var vehicle in new[] { volvo, transit, prius, leaf })
            {
                _vehicles.Add(vehicle);
            }

            _drivers.Add(new Driver("Ava Nguyen", "D-77120", LicenseClass.CE, now.AddYears(3), now.AddYears(-2)));
            _drivers.Add(new Driver("Liam Carter", "D-77121", LicenseClass.B, now.AddDays(21), now.AddYears(-1)));
            _drivers.Add(new Driver("Mia Rossi", "D-77122", LicenseClass.C, now.AddDays(-4), now.AddYears(-5)));

            volvo.CompleteMaintenance(now.AddDays(-200));
            _vehicles.Update(volvo);
            _maintenance.Add(new MaintenanceRecord(volvo.Id, MaintenanceType.FullService, now.AddDays(-200), 150000, new Money(920m, "USD"), "Annual service"));
            _maintenance.Add(new MaintenanceRecord(transit.Id, MaintenanceType.OilChange, now.AddDays(-45), 58000, new Money(110m, "USD")));

            var odometer = 60000.0;
            for (var i = 0; i < 5; i++)
            {
                odometer += 800;
                _fuelLogs.Add(new FuelLog(transit.Id, now.AddDays(-40 + i * 7), 82.5 + i, new Money(118m + i * 2, "USD"), odometer));
            }
        }
    }
}

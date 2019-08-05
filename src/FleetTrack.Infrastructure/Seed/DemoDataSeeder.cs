using System;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Infrastructure.Seed
{
    public class DemoDataSeeder
    {
        private readonly IVehicleRepository _vehicles;
        private readonly IDriverRepository _drivers;
        private readonly IClock _clock;

        public DemoDataSeeder(IVehicleRepository vehicles, IDriverRepository drivers, IClock clock)
        {
            _vehicles = vehicles;
            _drivers = drivers;
            _clock = clock;
        }

        public void Seed()
        {
            _vehicles.Add(new Vehicle("FT-1001", "Volvo", "FH16", 2017, FuelType.Diesel, 182000));
            _vehicles.Add(new Vehicle("FT-1002", "Ford", "Transit", 2018, FuelType.Diesel, 64000));
            _vehicles.Add(new Vehicle("FT-1003", "Toyota", "Prius", 2019, FuelType.Hybrid, 9000));

            var now = _clock.UtcNow;
            _drivers.Add(new Driver("Ava Nguyen", "D-77120", LicenseClass.CE, now.AddYears(3), now.AddYears(-2)));
            _drivers.Add(new Driver("Liam Carter", "D-77121", LicenseClass.B, now.AddDays(21), now.AddYears(-1)));
        }
    }
}

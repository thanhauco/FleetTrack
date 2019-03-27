using System;
using System.Collections.Generic;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Core.Services
{
    public class DriverService
    {
        private readonly IDriverRepository _drivers;
        private readonly IClock _clock;

        public DriverService(IDriverRepository drivers, IClock clock)
        {
            _drivers = Guard.NotNull(drivers, nameof(drivers));
            _clock = Guard.NotNull(clock, nameof(clock));
        }

        public Result<Driver> Hire(string fullName, string licenseNumber, LicenseClass licenseClass, DateTime licenseExpiryUtc)
        {
            if (_drivers.FindByLicense(licenseNumber) != null)
            {
                return Result.Fail<Driver>($"A driver with licence {licenseNumber} already exists.");
            }

            var now = _clock.UtcNow;
            if (licenseExpiryUtc.Date < now.Date)
            {
                return Result.Fail<Driver>("The licence has already expired.");
            }

            var driver = new Driver(fullName, licenseNumber, licenseClass, licenseExpiryUtc, now);
            _drivers.Add(driver);
            return Result.Ok(driver);
        }

        public Driver Get(Guid id)
        {
            return _drivers.Get(id);
        }

        public Result Deactivate(Guid id)
        {
            var driver = _drivers.Get(id);
            if (driver == null)
            {
                return Result.Fail("Driver not found.");
            }

            driver.Deactivate();
            _drivers.Update(driver);
            return Result.Ok();
        }

        public IReadOnlyList<Driver> GetWithExpiringLicenses(int withinDays)
        {
            var now = _clock.UtcNow;
            return _drivers.FindWithExpiringLicense(now, now.AddDays(withinDays));
        }
    }
}

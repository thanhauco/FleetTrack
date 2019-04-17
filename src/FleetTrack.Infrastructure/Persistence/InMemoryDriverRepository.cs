using System;
using System.Collections.Generic;
using System.Linq;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Infrastructure.Persistence
{
    public class InMemoryDriverRepository : InMemoryRepository<Driver>, IDriverRepository
    {
        public Driver FindByLicense(string licenseNumber)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber))
            {
                return null;
            }

            var normalized = licenseNumber.Trim();
            return Items.FirstOrDefault(d => string.Equals(d.LicenseNumber, normalized, StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<Driver> FindWithExpiringLicense(DateTime fromUtc, DateTime toUtc)
        {
            return Items
                .Where(d => d.IsActive && d.LicenseExpiryUtc.Date >= fromUtc.Date && d.LicenseExpiryUtc.Date <= toUtc.Date)
                .OrderBy(d => d.LicenseExpiryUtc)
                .ToList();
        }
    }
}

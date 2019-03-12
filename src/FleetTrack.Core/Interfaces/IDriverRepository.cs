using System;
using System.Collections.Generic;
using FleetTrack.Core.Domain;

namespace FleetTrack.Core.Interfaces
{
    public interface IDriverRepository : IRepository<Driver>
    {
        Driver FindByLicense(string licenseNumber);

        IReadOnlyList<Driver> FindWithExpiringLicense(DateTime fromUtc, DateTime toUtc);
    }
}

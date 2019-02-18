using System;
using FleetTrack.Core.Common;

namespace FleetTrack.Core.Domain
{
    public class Driver : Entity
    {
        public Driver(string fullName, string licenseNumber, LicenseClass licenseClass, DateTime licenseExpiryUtc, DateTime hiredUtc)
        {
            FullName = Guard.NotEmpty(fullName, nameof(fullName)).Trim();
            LicenseNumber = Guard.NotEmpty(licenseNumber, nameof(licenseNumber)).Trim().ToUpperInvariant();
            LicenseClass = licenseClass;
            LicenseExpiryUtc = licenseExpiryUtc;
            HiredUtc = hiredUtc;
            IsActive = true;
        }

        public string FullName { get; private set; }

        public string LicenseNumber { get; private set; }

        public LicenseClass LicenseClass { get; private set; }

        public DateTime LicenseExpiryUtc { get; private set; }

        public DateTime HiredUtc { get; private set; }

        public bool IsActive { get; private set; }

        public bool IsLicenseValid(DateTime nowUtc)
        {
            return IsActive && LicenseExpiryUtc.Date >= nowUtc.Date;
        }

        public int DaysUntilExpiry(DateTime nowUtc)
        {
            return (int)(LicenseExpiryUtc.Date - nowUtc.Date).TotalDays;
        }

        public void RenewLicense(DateTime newExpiryUtc)
        {
            if (newExpiryUtc <= LicenseExpiryUtc)
            {
                throw new DomainException("New expiry date must be after the current one.");
            }

            LicenseExpiryUtc = newExpiryUtc;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }
    }
}

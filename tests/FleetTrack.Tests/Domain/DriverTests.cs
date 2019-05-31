using System;
using FleetTrack.Core.Common;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Domain
{
    public class DriverTests
    {
        [Fact]
        public void IsLicenseValid_TrueUntilExpiryDay()
        {
            var driver = TestData.Driver(expiresInDays: 0);

            Assert.True(driver.IsLicenseValid(TestData.Now));
            Assert.False(driver.IsLicenseValid(TestData.Now.AddDays(1)));
        }

        [Fact]
        public void IsLicenseValid_FalseWhenInactive()
        {
            var driver = TestData.Driver();
            driver.Deactivate();

            Assert.False(driver.IsLicenseValid(TestData.Now));
        }

        [Fact]
        public void DaysUntilExpiry_CanBeNegative()
        {
            var driver = TestData.Driver(expiresInDays: -3);

            Assert.Equal(-3, driver.DaysUntilExpiry(TestData.Now));
        }

        [Fact]
        public void RenewLicense_MustExtendExpiry()
        {
            var driver = TestData.Driver(expiresInDays: 10);

            Assert.Throws<DomainException>(() => driver.RenewLicense(TestData.Now.AddDays(5)));

            driver.RenewLicense(TestData.Now.AddDays(400));
            Assert.Equal(400, driver.DaysUntilExpiry(TestData.Now));
        }
    }
}

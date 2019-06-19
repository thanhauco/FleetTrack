using System;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Services;
using FleetTrack.Infrastructure.Persistence;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Services
{
    public class DriverServiceTests
    {
        private readonly InMemoryDriverRepository _repo = new InMemoryDriverRepository();
        private readonly FakeClock _clock = new FakeClock(TestData.Now);
        private readonly DriverService _service;

        public DriverServiceTests()
        {
            _service = new DriverService(_repo, _clock);
        }

        [Fact]
        public void Hire_AddsDriver()
        {
            var result = _service.Hire("Ava", "d-1", LicenseClass.B, TestData.Now.AddYears(2));

            Assert.True(result.IsSuccess);
            Assert.Equal("D-1", result.Value.LicenseNumber);
            Assert.Equal(TestData.Now, result.Value.HiredUtc);
        }

        [Fact]
        public void Hire_RejectsExpiredLicence()
        {
            var result = _service.Hire("Ava", "D-1", LicenseClass.B, TestData.Now.AddDays(-1));

            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Hire_RejectsDuplicateLicence()
        {
            _service.Hire("Ava", "D-1", LicenseClass.B, TestData.Now.AddYears(1));

            Assert.True(_service.Hire("Liam", "d-1", LicenseClass.C, TestData.Now.AddYears(1)).IsFailure);
        }

        [Fact]
        public void Deactivate_MarksDriverInactive()
        {
            var driver = _service.Hire("Ava", "D-1", LicenseClass.B, TestData.Now.AddYears(1)).Value;

            Assert.True(_service.Deactivate(driver.Id).IsSuccess);
            Assert.False(_repo.Get(driver.Id).IsActive);
            Assert.True(_service.Deactivate(Guid.NewGuid()).IsFailure);
        }

        [Fact]
        public void GetWithExpiringLicenses_ReturnsOnlyWithinWindow()
        {
            _service.Hire("Soon", "D-1", LicenseClass.B, TestData.Now.AddDays(10));
            _service.Hire("Later", "D-2", LicenseClass.B, TestData.Now.AddDays(200));

            var expiring = _service.GetWithExpiringLicenses(30);

            Assert.Single(expiring);
            Assert.Equal("Soon", expiring[0].FullName);
        }
    }
}

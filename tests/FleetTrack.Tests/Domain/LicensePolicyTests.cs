using System;
using FleetTrack.Core.Domain;
using Xunit;

namespace FleetTrack.Tests.Domain
{
    public class LicensePolicyTests
    {
        [Theory]
        [InlineData(LicenseClass.B, VehicleCategory.Car, true)]
        [InlineData(LicenseClass.B, VehicleCategory.Van, true)]
        [InlineData(LicenseClass.B, VehicleCategory.Truck, false)]
        [InlineData(LicenseClass.B, VehicleCategory.ArticulatedTruck, false)]
        [InlineData(LicenseClass.B, VehicleCategory.Bus, false)]
        [InlineData(LicenseClass.C, VehicleCategory.Car, true)]
        [InlineData(LicenseClass.C, VehicleCategory.Truck, true)]
        [InlineData(LicenseClass.C, VehicleCategory.ArticulatedTruck, false)]
        [InlineData(LicenseClass.CE, VehicleCategory.Truck, true)]
        [InlineData(LicenseClass.CE, VehicleCategory.ArticulatedTruck, true)]
        [InlineData(LicenseClass.CE, VehicleCategory.Bus, false)]
        [InlineData(LicenseClass.D, VehicleCategory.Bus, true)]
        [InlineData(LicenseClass.D, VehicleCategory.Truck, false)]
        [InlineData(LicenseClass.D, VehicleCategory.Van, true)]
        public void CanDrive_FollowsLicenceHierarchy(LicenseClass licence, VehicleCategory category, bool expected)
        {
            Assert.Equal(expected, LicensePolicy.CanDrive(licence, category));
        }

        [Fact]
        public void MinimumFor_IsAlwaysSufficient()
        {
            foreach (VehicleCategory category in Enum.GetValues(typeof(VehicleCategory)))
            {
                Assert.True(LicensePolicy.CanDrive(LicensePolicy.MinimumFor(category), category));
            }
        }

        [Fact]
        public void UnknownCategory_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LicensePolicy.CanDrive(LicenseClass.CE, (VehicleCategory)99));
        }

        [Fact]
        public void Vehicle_DefaultsToCar_AndRejectsUnknownCategory()
        {
            Assert.Equal(VehicleCategory.Car, new Vehicle("A-1", "Toyota", "Prius", 2019, FuelType.Hybrid, 0).Category);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Vehicle("A-1", "Volvo", "FH16", 2019, FuelType.Diesel, 0, (VehicleCategory)42));
        }
    }
}

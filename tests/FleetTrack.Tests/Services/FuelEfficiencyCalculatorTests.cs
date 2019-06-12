using System;
using System.Collections.Generic;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Services;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Services
{
    public class FuelEfficiencyCalculatorTests
    {
        private static FuelLog Log(double liters, double odometer)
        {
            return new FuelLog(Guid.NewGuid(), TestData.Now, liters, TestData.Usd(1m), odometer);
        }

        [Fact]
        public void LitersPer100Km_NullWithSingleLog()
        {
            Assert.Null(FuelEfficiencyCalculator.LitersPer100Km(new List<FuelLog> { Log(40, 1000) }));
        }

        [Fact]
        public void LitersPer100Km_IgnoresFirstFill()
        {
            var logs = new List<FuelLog> { Log(50, 1000), Log(40, 1500), Log(35, 2000) };

            var result = FuelEfficiencyCalculator.LitersPer100Km(logs);

            Assert.Equal(7.5, result.Value, 6);
        }

        [Fact]
        public void LitersPer100Km_SortsByOdometer()
        {
            var logs = new List<FuelLog> { Log(35, 2000), Log(50, 1000), Log(40, 1500) };

            Assert.Equal(7.5, FuelEfficiencyCalculator.LitersPer100Km(logs).Value, 6);
        }

        [Fact]
        public void KmPerLiter_IsInverseOfConsumption()
        {
            var logs = new List<FuelLog> { Log(50, 1000), Log(50, 1500) };

            Assert.Equal(10.0, FuelEfficiencyCalculator.KmPerLiter(logs).Value, 6);
        }

        [Fact]
        public void KmPerLiter_NullWhenNoDistance()
        {
            var logs = new List<FuelLog> { Log(50, 1000), Log(50, 1000) };

            Assert.Null(FuelEfficiencyCalculator.KmPerLiter(logs));
        }
    }
}

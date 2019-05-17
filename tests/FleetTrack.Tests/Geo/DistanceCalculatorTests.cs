using FleetTrack.Core.Domain;
using FleetTrack.Core.Geo;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Geo
{
    public class DistanceCalculatorTests
    {
        [Fact]
        public void SamePoint_IsZero()
        {
            Assert.Equal(0.0, DistanceCalculator.KilometersBetween(TestData.Berlin, TestData.Berlin), 6);
        }

        [Fact]
        public void BerlinToMunich_IsAbout504Km()
        {
            var km = DistanceCalculator.KilometersBetween(TestData.Berlin, TestData.Munich);

            Assert.InRange(km, 495, 515);
        }

        [Fact]
        public void Distance_IsSymmetric()
        {
            var forward = DistanceCalculator.KilometersBetween(TestData.Berlin, TestData.Munich);
            var backward = DistanceCalculator.KilometersBetween(TestData.Munich, TestData.Berlin);

            Assert.Equal(forward, backward, 9);
        }

        [Fact]
        public void QuarterOfEquator_IsAbout10007Km()
        {
            var km = DistanceCalculator.KilometersBetween(new GeoPoint(0, 0), new GeoPoint(0, 90));

            Assert.InRange(km, 10000, 10015);
        }
    }
}

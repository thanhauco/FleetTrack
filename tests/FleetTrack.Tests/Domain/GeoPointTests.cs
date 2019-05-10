using System;
using FleetTrack.Core.Domain;
using Xunit;

namespace FleetTrack.Tests.Domain
{
    public class GeoPointTests
    {
        [Theory]
        [InlineData(91, 0)]
        [InlineData(-91, 0)]
        [InlineData(0, 181)]
        [InlineData(0, -181)]
        public void Constructor_Throws_ForOutOfRangeCoordinates(double lat, double lon)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GeoPoint(lat, lon));
        }

        [Fact]
        public void Equality_IsByValue()
        {
            Assert.Equal(new GeoPoint(10, 20), new GeoPoint(10, 20));
            Assert.True(new GeoPoint(10, 20) != new GeoPoint(10, 21));
        }

        [Fact]
        public void ToString_UsesInvariantCulture()
        {
            Assert.Equal("(52.52000, 13.40500)", new GeoPoint(52.52, 13.405).ToString());
        }
    }
}

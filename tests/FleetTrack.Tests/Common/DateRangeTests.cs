using System;
using FleetTrack.Core.Common;
using Xunit;

namespace FleetTrack.Tests.Common
{
    public class DateRangeTests
    {
        private static readonly DateTime Start = new DateTime(2019, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void Constructor_Throws_WhenEndBeforeStart()
        {
            Assert.Throws<ArgumentException>(() => new DateRange(Start, Start.AddDays(-1)));
        }

        [Fact]
        public void Contains_IsInclusive()
        {
            var range = new DateRange(Start, Start.AddDays(10));

            Assert.True(range.Contains(Start));
            Assert.True(range.Contains(Start.AddDays(10)));
            Assert.False(range.Contains(Start.AddDays(11)));
        }

        [Fact]
        public void TotalHours_IsComputed()
        {
            var range = new DateRange(Start, Start.AddDays(2));

            Assert.Equal(48.0, range.TotalHours);
        }
    }
}

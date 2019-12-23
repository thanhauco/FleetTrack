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

        [Fact]
        public void Overlap_IsClippedToRange()
        {
            var range = new DateRange(Start, Start.AddHours(10));

            Assert.Equal(TimeSpan.FromHours(2), range.Overlap(Start.AddHours(8), Start.AddHours(30)));
            Assert.Equal(TimeSpan.FromHours(3), range.Overlap(Start.AddHours(-5), Start.AddHours(3)));
            Assert.Equal(TimeSpan.FromHours(10), range.Overlap(Start.AddHours(-5), Start.AddHours(50)));
            Assert.Equal(TimeSpan.Zero, range.Overlap(Start.AddHours(11), Start.AddHours(12)));
        }

        [Fact]
        public void Intersects_IncludesTouchingIntervals()
        {
            var range = new DateRange(Start, Start.AddHours(10));

            Assert.True(range.Intersects(Start.AddHours(-2), Start));
            Assert.True(range.Intersects(Start.AddHours(10), Start.AddHours(12)));
            Assert.False(range.Intersects(Start.AddHours(11), Start.AddHours(12)));
        }
    }
}

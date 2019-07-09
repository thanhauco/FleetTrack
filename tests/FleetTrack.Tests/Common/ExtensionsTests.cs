using System;
using System.Linq;
using FleetTrack.Core.Common;
using Xunit;

namespace FleetTrack.Tests.Common
{
    public class ExtensionsTests
    {
        [Theory]
        [InlineData("Volvo FH16", "volvo", true)]
        [InlineData("Volvo FH16", "FH", true)]
        [InlineData("Volvo FH16", "scania", false)]
        [InlineData(null, "x", false)]
        public void ContainsIgnoreCase_Works(string source, string value, bool expected)
        {
            Assert.Equal(expected, source.ContainsIgnoreCase(value));
        }

        [Fact]
        public void StartOfMonth_ResetsDayAndTime()
        {
            var value = new DateTime(2019, 8, 17, 13, 45, 10, DateTimeKind.Utc);

            Assert.Equal(new DateTime(2019, 8, 1, 0, 0, 0, DateTimeKind.Utc), value.StartOfMonth());
        }

        [Fact]
        public void EndOfDay_IsLastTickOfTheDay()
        {
            var value = new DateTime(2019, 8, 17, 13, 45, 10);

            Assert.Equal(new DateTime(2019, 8, 17).AddDays(1).AddTicks(-1), value.EndOfDay());
        }

        [Fact]
        public void Batch_SplitsSequence()
        {
            var batches = Enumerable.Range(1, 7).Batch(3).ToList();

            Assert.Equal(3, batches.Count);
            Assert.Equal(new[] { 1, 2, 3 }, batches[0]);
            Assert.Equal(new[] { 7 }, batches[2]);
        }

        [Fact]
        public void Batch_Throws_ForNonPositiveSize()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Enumerable.Range(1, 3).Batch(0).ToList());
        }
    }
}

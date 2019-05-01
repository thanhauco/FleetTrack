using System;
using FleetTrack.Core.Common;
using Xunit;

namespace FleetTrack.Tests.Common
{
    public class GuardTests
    {
        [Fact]
        public void NotNull_ReturnsValue_WhenNotNull()
        {
            Assert.Equal("x", Guard.NotNull("x", "arg"));
        }

        [Fact]
        public void NotNull_Throws_WhenNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Guard.NotNull<string>(null, "arg"));
            Assert.Equal("arg", ex.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NotEmpty_Throws_ForBlankStrings(string value)
        {
            Assert.Throws<ArgumentException>(() => Guard.NotEmpty(value, "arg"));
        }

        [Fact]
        public void Positive_Throws_ForZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Positive(0, "arg"));
            Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Positive(0.0, "arg"));
        }

        [Fact]
        public void NotNegative_AllowsZero()
        {
            Assert.Equal(0.0, Guard.NotNegative(0.0, "arg"));
            Assert.Equal(0m, Guard.NotNegative(0m, "arg"));
        }

        [Fact]
        public void InRange_Throws_OutsideBounds()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Guard.InRange(91.0, -90.0, 90.0, "lat"));
            Assert.Throws<ArgumentOutOfRangeException>(() => Guard.InRange(1979, 1980, 2100, "year"));
        }
    }
}

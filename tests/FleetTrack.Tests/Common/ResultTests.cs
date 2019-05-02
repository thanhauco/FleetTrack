using FleetTrack.Core.Common;
using Xunit;

namespace FleetTrack.Tests.Common
{
    public class ResultTests
    {
        [Fact]
        public void Ok_IsSuccess()
        {
            var result = Result.Ok();

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Null(result.Error);
        }

        [Fact]
        public void Fail_CarriesError()
        {
            var result = Result.Fail("boom");

            Assert.True(result.IsFailure);
            Assert.Equal("boom", result.Error);
        }

        [Fact]
        public void GenericOk_CarriesValue()
        {
            var result = Result.Ok(42);

            Assert.True(result.IsSuccess);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void GenericFail_HasDefaultValue()
        {
            var result = Result.Fail<string>("nope");

            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.Equal("nope", result.Error);
        }
    }
}

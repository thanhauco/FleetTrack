using System.Linq;
using FleetTrack.Core.Common;
using Xunit;

namespace FleetTrack.Tests.Common
{
    public class PagedResultTests
    {
        [Fact]
        public void Create_ReturnsRequestedPage()
        {
            var result = PagedResult<int>.Create(Enumerable.Range(1, 25), 2, 10);

            Assert.Equal(Enumerable.Range(11, 10), result.Items);
            Assert.Equal(25, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.True(result.HasNext);
        }

        [Fact]
        public void Create_LastPageHasRemainder()
        {
            var result = PagedResult<int>.Create(Enumerable.Range(1, 25), 3, 10);

            Assert.Equal(5, result.Items.Count);
            Assert.False(result.HasNext);
        }

        [Fact]
        public void Create_BeyondLastPage_ReturnsEmpty()
        {
            var result = PagedResult<int>.Create(Enumerable.Range(1, 5), 4, 10);

            Assert.Empty(result.Items);
            Assert.Equal(5, result.TotalCount);
        }
    }
}

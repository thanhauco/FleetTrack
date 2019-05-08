using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using Xunit;

namespace FleetTrack.Tests.Domain
{
    public class MoneyTests
    {
        [Fact]
        public void Constructor_RoundsToTwoDecimals()
        {
            Assert.Equal(10.13m, new Money(10.125m, "usd").Amount);
        }

        [Fact]
        public void Constructor_NormalizesCurrency()
        {
            Assert.Equal("EUR", new Money(1m, " eur ").Currency);
        }

        [Fact]
        public void Add_SameCurrency()
        {
            var sum = new Money(10m, "USD") + new Money(5.5m, "USD");

            Assert.Equal(new Money(15.5m, "USD"), sum);
        }

        [Fact]
        public void Add_DifferentCurrency_Throws()
        {
            Assert.Throws<DomainException>(() => new Money(1m, "USD") + new Money(1m, "EUR"));
        }

        [Fact]
        public void Multiply_ScalesAmount()
        {
            Assert.Equal(new Money(30m, "USD"), new Money(10m, "USD") * 3m);
        }

        [Fact]
        public void Equality_ConsidersCurrency()
        {
            Assert.True(new Money(1m, "USD") == new Money(1m, "USD"));
            Assert.True(new Money(1m, "USD") != new Money(1m, "EUR"));
        }

        [Fact]
        public void ToString_IsInvariant()
        {
            Assert.Equal("1234.50 USD", new Money(1234.5m, "USD").ToString());
        }
    }
}

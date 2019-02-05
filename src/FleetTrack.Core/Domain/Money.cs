using System;
using System.Globalization;
using FleetTrack.Core.Common;

namespace FleetTrack.Core.Domain
{
    public readonly struct Money : IEquatable<Money>
    {
        public Money(decimal amount, string currency)
        {
            Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
            Currency = Guard.NotEmpty(currency, nameof(currency)).Trim().ToUpperInvariant();
        }

        public decimal Amount { get; }

        public string Currency { get; }

        public static Money Zero(string currency)
        {
            return new Money(0m, currency);
        }

        public static Money operator +(Money left, Money right)
        {
            EnsureSameCurrency(left, right);
            return new Money(left.Amount + right.Amount, left.Currency);
        }

        public static Money operator -(Money left, Money right)
        {
            EnsureSameCurrency(left, right);
            return new Money(left.Amount - right.Amount, left.Currency);
        }

        public static Money operator *(Money money, decimal factor)
        {
            return new Money(money.Amount * factor, money.Currency);
        }

        public static bool operator ==(Money left, Money right) => left.Equals(right);

        public static bool operator !=(Money left, Money right) => !left.Equals(right);

        public bool Equals(Money other)
        {
            return Amount == other.Amount && string.Equals(Currency, other.Currency, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is Money other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Amount.GetHashCode() * 397) ^ (Currency != null ? Currency.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:F2} {1}", Amount, Currency);
        }

        private static void EnsureSameCurrency(Money left, Money right)
        {
            if (!string.Equals(left.Currency, right.Currency, StringComparison.Ordinal))
            {
                throw new DomainException($"Currency mismatch: {left.Currency} vs {right.Currency}.");
            }
        }
    }
}

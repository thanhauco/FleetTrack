using System;

namespace FleetTrack.Core.Common
{
    public readonly struct DateRange
    {
        public DateRange(DateTime fromUtc, DateTime toUtc)
        {
            if (toUtc < fromUtc)
            {
                throw new ArgumentException("End of range cannot be before its start.", nameof(toUtc));
            }

            FromUtc = fromUtc;
            ToUtc = toUtc;
        }

        public DateTime FromUtc { get; }

        public DateTime ToUtc { get; }

        public double TotalHours => (ToUtc - FromUtc).TotalHours;

        public bool Contains(DateTime value)
        {
            return value >= FromUtc && value <= ToUtc;
        }

        public bool Intersects(DateTime fromUtc, DateTime toUtc)
        {
            return fromUtc <= ToUtc && toUtc >= FromUtc;
        }

        /// <summary>
        /// Length of the part of [fromUtc, toUtc] that falls inside this range.
        /// </summary>
        public TimeSpan Overlap(DateTime fromUtc, DateTime toUtc)
        {
            var start = fromUtc > FromUtc ? fromUtc : FromUtc;
            var end = toUtc < ToUtc ? toUtc : ToUtc;
            return end > start ? end - start : TimeSpan.Zero;
        }

        public override string ToString()
        {
            return $"{FromUtc:yyyy-MM-dd} .. {ToUtc:yyyy-MM-dd}";
        }
    }
}

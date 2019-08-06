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

        public override string ToString()
        {
            return $"{FromUtc:yyyy-MM-dd} .. {ToUtc:yyyy-MM-dd}";
        }
    }
}

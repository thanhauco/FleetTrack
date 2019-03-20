using System;
using System.Collections.Generic;
using System.Linq;

namespace FleetTrack.Core.Common
{
    public static class StringExtensions
    {
        public static bool ContainsIgnoreCase(this string source, string value)
        {
            if (source == null || value == null)
            {
                return false;
            }

            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1, 0, 0, 0, value.Kind);
        }

        public static DateTime EndOfDay(this DateTime value)
        {
            return value.Date.AddDays(1).AddTicks(-1);
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<IReadOnlyList<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            Guard.NotNull(source, nameof(source));
            Guard.Positive(size, nameof(size));

            var bucket = new List<T>(size);
            foreach (var item in source)
            {
                bucket.Add(item);
                if (bucket.Count == size)
                {
                    yield return bucket;
                    bucket = new List<T>(size);
                }
            }

            if (bucket.Count > 0)
            {
                yield return bucket;
            }
        }
    }
}

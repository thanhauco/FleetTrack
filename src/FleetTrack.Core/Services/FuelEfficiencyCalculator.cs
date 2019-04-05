using System.Collections.Generic;
using System.Linq;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;

namespace FleetTrack.Core.Services
{
    /// <summary>
    /// Computes fuel consumption with the "full tank to full tank" method: the
    /// first fill-up only sets the baseline odometer, all later fills count.
    /// </summary>
    public static class FuelEfficiencyCalculator
    {
        public static double? LitersPer100Km(IReadOnlyList<FuelLog> logs)
        {
            Guard.NotNull(logs, nameof(logs));

            if (logs.Count < 2)
            {
                return null;
            }

            var ordered = logs.OrderBy(l => l.OdometerKm).ToList();
            var distance = ordered[ordered.Count - 1].OdometerKm - ordered[0].OdometerKm;
            if (distance <= 0)
            {
                return null;
            }

            var liters = ordered.Skip(1).Sum(l => l.Liters);
            return liters / distance * 100.0;
        }

        public static double? KmPerLiter(IReadOnlyList<FuelLog> logs)
        {
            var consumption = LitersPer100Km(logs);
            if (!consumption.HasValue || consumption.Value <= 0)
            {
                return null;
            }

            return 100.0 / consumption.Value;
        }
    }
}

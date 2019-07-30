using System.Globalization;
using FleetTrack.Core.Common;
using FleetTrack.Core.Events;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Core.Handlers
{
    /// <summary>
    /// Sends a notification whenever a completed trip exceeds a distance threshold.
    /// </summary>
    public class LongTripNotifier
    {
        private readonly INotifier _notifier;
        private readonly double _thresholdKm;

        public LongTripNotifier(IEventBus bus, INotifier notifier, double thresholdKm = 500)
        {
            Guard.NotNull(bus, nameof(bus));
            _notifier = Guard.NotNull(notifier, nameof(notifier));
            _thresholdKm = thresholdKm;

            bus.Subscribe<TripCompletedEvent>(OnTripCompleted);
        }

        private void OnTripCompleted(TripCompletedEvent e)
        {
            if (e.DistanceKm < _thresholdKm)
            {
                return;
            }

            var message = string.Format(
                CultureInfo.InvariantCulture,
                "Trip {0} covered {1:F1} km (threshold {2:F0} km).",
                e.TripId,
                e.DistanceKm,
                _thresholdKm);

            _notifier.Notify("Long trip completed", message);
        }
    }
}

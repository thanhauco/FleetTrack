using System;
using System.Collections.Generic;
using FleetTrack.Core.Events;
using FleetTrack.Core.Handlers;
using FleetTrack.Core.Interfaces;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Handlers
{
    public class LongTripNotifierTests
    {
        private class RecordingNotifier : INotifier
        {
            public List<string> Subjects { get; } = new List<string>();

            public List<string> Messages { get; } = new List<string>();

            public void Notify(string subject, string message)
            {
                Subjects.Add(subject);
                Messages.Add(message);
            }
        }

        private static TripCompletedEvent Event(double km)
        {
            return new TripCompletedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), km, TestData.Now);
        }

        [Fact]
        public void LongTrip_TriggersNotification()
        {
            var bus = new InProcessEventBus();
            var notifier = new RecordingNotifier();
            var handler = new LongTripNotifier(bus, notifier, 500);

            bus.Publish(Event(620.25));

            Assert.Single(notifier.Subjects);
            Assert.Contains("threshold 500 km", notifier.Messages[0]);
        }

        [Fact]
        public void ShortTrip_IsIgnored()
        {
            var bus = new InProcessEventBus();
            var notifier = new RecordingNotifier();
            var handler = new LongTripNotifier(bus, notifier, 500);

            bus.Publish(Event(120));

            Assert.Empty(notifier.Subjects);
        }

        [Fact]
        public void ThresholdIsInclusive()
        {
            var bus = new InProcessEventBus();
            var notifier = new RecordingNotifier();
            var handler = new LongTripNotifier(bus, notifier, 300);

            bus.Publish(Event(300));

            Assert.Single(notifier.Subjects);
        }
    }
}

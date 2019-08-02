using System;
using FleetTrack.Core.Events;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Events
{
    public class InProcessEventBusTests
    {
        private static TripCompletedEvent NewEvent(double km = 10)
        {
            return new TripCompletedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), km, TestData.Now);
        }

        [Fact]
        public void Publish_WithoutSubscribers_DoesNothing()
        {
            new InProcessEventBus().Publish(NewEvent());
        }

        [Fact]
        public void Publish_DeliversToAllSubscribers()
        {
            var bus = new InProcessEventBus();
            var calls = 0;
            bus.Subscribe<TripCompletedEvent>(e => calls++);
            bus.Subscribe<TripCompletedEvent>(e => calls += 10);

            bus.Publish(NewEvent());

            Assert.Equal(11, calls);
        }

        [Fact]
        public void Publish_PassesTheEvent()
        {
            var bus = new InProcessEventBus();
            TripCompletedEvent received = null;
            bus.Subscribe<TripCompletedEvent>(e => received = e);
            var published = NewEvent(42);

            bus.Publish(published);

            Assert.Same(published, received);
        }

        [Fact]
        public void Subscribe_NullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new InProcessEventBus().Subscribe<TripCompletedEvent>(null));
        }
    }
}

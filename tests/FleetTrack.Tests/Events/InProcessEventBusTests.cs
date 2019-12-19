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
        public void FailingHandler_DoesNotStopOtherHandlers_AndIsReported()
        {
            IDomainEvent failedEvent = null;
            Exception failure = null;
            var bus = new InProcessEventBus((e, ex) =>
            {
                failedEvent = e;
                failure = ex;
            });
            var laterHandlerRan = false;
            bus.Subscribe<TripCompletedEvent>(e => { throw new InvalidOperationException("smtp down"); });
            bus.Subscribe<TripCompletedEvent>(e => laterHandlerRan = true);
            var published = NewEvent();

            bus.Publish(published);

            Assert.True(laterHandlerRan);
            Assert.Same(published, failedEvent);
            Assert.Equal("smtp down", failure.Message);
        }

        [Fact]
        public void FailingHandler_WithDefaultReporter_DoesNotThrow()
        {
            var bus = new InProcessEventBus();
            bus.Subscribe<TripCompletedEvent>(e => { throw new InvalidOperationException(); });

            Assert.Null(Record.Exception(() => bus.Publish(NewEvent())));
        }

        [Fact]
        public void Subscribe_NullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new InProcessEventBus().Subscribe<TripCompletedEvent>(null));
        }
    }
}

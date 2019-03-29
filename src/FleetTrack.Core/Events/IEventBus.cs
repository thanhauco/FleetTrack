using System;

namespace FleetTrack.Core.Events
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent @event) where TEvent : IDomainEvent;

        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent;
    }
}

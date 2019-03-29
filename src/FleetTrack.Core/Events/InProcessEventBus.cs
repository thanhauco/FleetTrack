using System;
using System.Collections.Generic;
using System.Linq;
using FleetTrack.Core.Common;

namespace FleetTrack.Core.Events
{
    public class InProcessEventBus : IEventBus
    {
        private readonly object _gate = new object();
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Publish<TEvent>(TEvent @event) where TEvent : IDomainEvent
        {
            Guard.NotNull((object)@event, nameof(@event));

            Delegate[] snapshot;
            lock (_gate)
            {
                List<Delegate> list;
                if (!_handlers.TryGetValue(typeof(TEvent), out list))
                {
                    return;
                }

                snapshot = list.ToArray();
            }

            foreach (var handler in snapshot)
            {
                ((Action<TEvent>)handler)(@event);
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent
        {
            Guard.NotNull(handler, nameof(handler));

            lock (_gate)
            {
                List<Delegate> list;
                if (!_handlers.TryGetValue(typeof(TEvent), out list))
                {
                    list = new List<Delegate>();
                    _handlers[typeof(TEvent)] = list;
                }

                list.Add(handler);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FleetTrack.Core.Common;

namespace FleetTrack.Core.Events
{
    /// <summary>
    /// Synchronous, in-process event dispatch.
    /// </summary>
    /// <remarks>
    /// Events are published after the state change they describe has been applied,
    /// so a failing subscriber cannot undo it. Letting the exception escape would
    /// make the caller report a failure for an operation that succeeded and would
    /// skip the remaining subscribers. Handler failures are therefore isolated and
    /// reported through <c>onHandlerError</c> instead.
    /// </remarks>
    public class InProcessEventBus : IEventBus
    {
        private readonly object _gate = new object();
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        private readonly Action<IDomainEvent, Exception> _onHandlerError;

        public InProcessEventBus(Action<IDomainEvent, Exception> onHandlerError = null)
        {
            _onHandlerError = onHandlerError ?? TraceHandlerError;
        }

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
                try
                {
                    ((Action<TEvent>)handler)(@event);
                }
                catch (Exception ex)
                {
                    _onHandlerError(@event, ex);
                }
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

        private static void TraceHandlerError(IDomainEvent @event, Exception ex)
        {
            Trace.TraceError("Handler for {0} failed: {1}", @event.GetType().Name, ex);
        }
    }
}

using System;

namespace FleetTrack.Core.Events
{
    public interface IDomainEvent
    {
        DateTime OccurredUtc { get; }
    }
}

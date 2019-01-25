using System;

namespace FleetTrack.Core.Common
{
    /// <summary>
    /// Abstraction over the system clock so time-dependent logic stays testable.
    /// </summary>
    public interface IClock
    {
        DateTime UtcNow { get; }
    }

    public class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}

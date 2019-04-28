using System;
using FleetTrack.Core.Common;

namespace FleetTrack.Tests.Helpers
{
    public class FakeClock : IClock
    {
        public FakeClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; set; }

        public void Advance(TimeSpan by)
        {
            UtcNow = UtcNow.Add(by);
        }
    }
}

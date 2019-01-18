using System;

namespace FleetTrack.Core.Common
{
    /// <summary>
    /// Raised when a business rule of the fleet domain is violated.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message)
            : base(message)
        {
        }

        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

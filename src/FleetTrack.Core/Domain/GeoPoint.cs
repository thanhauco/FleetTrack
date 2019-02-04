using System;
using System.Globalization;
using FleetTrack.Core.Common;

namespace FleetTrack.Core.Domain
{
    public readonly struct GeoPoint : IEquatable<GeoPoint>
    {
        public GeoPoint(double latitude, double longitude)
        {
            Latitude = Guard.InRange(latitude, -90.0, 90.0, nameof(latitude));
            Longitude = Guard.InRange(longitude, -180.0, 180.0, nameof(longitude));
        }

        public double Latitude { get; }

        public double Longitude { get; }

        public bool Equals(GeoPoint other)
        {
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
        }

        public override bool Equals(object obj)
        {
            return obj is GeoPoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Latitude.GetHashCode() * 397) ^ Longitude.GetHashCode();
            }
        }

        public static bool operator ==(GeoPoint left, GeoPoint right) => left.Equals(right);

        public static bool operator !=(GeoPoint left, GeoPoint right) => !left.Equals(right);

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0:F5}, {1:F5})", Latitude, Longitude);
        }
    }
}

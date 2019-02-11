using System;
using FleetTrack.Core.Domain;

namespace FleetTrack.Core.Geo
{
    public static class DistanceCalculator
    {
        private const double EarthRadiusKm = 6371.0088;

        /// <summary>
        /// Great-circle distance between two points using the haversine formula.
        /// </summary>
        public static double KilometersBetween(GeoPoint from, GeoPoint to)
        {
            var dLat = ToRadians(to.Latitude - from.Latitude);
            var dLon = ToRadians(to.Longitude - from.Longitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(from.Latitude)) * Math.Cos(ToRadians(to.Latitude)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}

using System;
using FleetTrack.Core.Domain;

namespace FleetTrack.Tests.Helpers
{
    public static class TestData
    {
        public static readonly DateTime Now = new DateTime(2019, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        public static Vehicle Vehicle(string plate = "AB-123", double odometerKm = 1000, FuelType fuel = FuelType.Diesel)
        {
            return new Vehicle(plate, "Volvo", "FH16", 2018, fuel, odometerKm);
        }

        public static Driver Driver(string license = "D-1", int expiresInDays = 365)
        {
            return new Driver("Test Driver", license, LicenseClass.B, Now.AddDays(expiresInDays), Now.AddYears(-1));
        }

        public static GeoPoint Berlin => new GeoPoint(52.5200, 13.4050);

        public static GeoPoint Munich => new GeoPoint(48.1351, 11.5820);

        public static Money Usd(decimal amount)
        {
            return new Money(amount, "USD");
        }
    }
}

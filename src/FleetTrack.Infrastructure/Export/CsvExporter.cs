using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FleetTrack.Core.Domain;

namespace FleetTrack.Infrastructure.Export
{
    public static class CsvExporter
    {
        public static string ExportVehicles(IEnumerable<Vehicle> vehicles)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id,LicensePlate,Make,Model,Year,FuelType,Status,OdometerKm");

            foreach (var v in vehicles)
            {
                sb.AppendLine(string.Join(",",
                    v.Id,
                    Escape(v.LicensePlate),
                    Escape(v.Make),
                    Escape(v.Model),
                    v.Year.ToString(CultureInfo.InvariantCulture),
                    v.FuelType,
                    v.Status,
                    v.OdometerKm.ToString("F1", CultureInfo.InvariantCulture)));
            }

            return sb.ToString();
        }

        public static string ExportDrivers(IEnumerable<Driver> drivers)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id,FullName,LicenseNumber,LicenseClass,LicenseExpiry,IsActive");

            foreach (var d in drivers)
            {
                sb.AppendLine(string.Join(",",
                    d.Id,
                    Escape(d.FullName),
                    Escape(d.LicenseNumber),
                    d.LicenseClass,
                    d.LicenseExpiryUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    d.IsActive));
            }

            return sb.ToString();
        }

        public static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}

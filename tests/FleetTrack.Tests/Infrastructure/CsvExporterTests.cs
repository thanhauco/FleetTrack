using System;
using System.Linq;
using FleetTrack.Core.Domain;
using FleetTrack.Infrastructure.Export;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Infrastructure
{
    public class CsvExporterTests
    {
        [Theory]
        [InlineData("plain", "plain")]
        [InlineData("a,b", "\"a,b\"")]
        [InlineData("say \"hi\"", "\"say \"\"hi\"\"\"")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void Escape_QuotesWhenNeeded(string input, string expected)
        {
            Assert.Equal(expected, CsvExporter.Escape(input));
        }

        [Fact]
        public void ExportVehicles_WritesHeaderAndRows()
        {
            var vehicle = new Vehicle("AB-1", "Mercedes-Benz, AG", "Sprinter", 2018, FuelType.Diesel, 1234.56);

            var lines = CsvExporter.ExportVehicles(new[] { vehicle })
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(2, lines.Length);
            Assert.Equal("Id,LicensePlate,Make,Model,Year,FuelType,Status,OdometerKm", lines[0]);
            Assert.Equal(vehicle.Id + ",AB-1,\"Mercedes-Benz, AG\",Sprinter,2018,Diesel,Available,1234.6", lines[1]);
        }

        [Fact]
        public void ExportDrivers_WritesIsoDates()
        {
            var driver = TestData.Driver("D-9", 0);

            var csv = CsvExporter.ExportDrivers(new[] { driver });

            Assert.Contains("2019-06-15", csv);
            Assert.Contains("D-9", csv);
        }
    }
}

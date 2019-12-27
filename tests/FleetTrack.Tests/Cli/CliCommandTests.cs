using System;
using System.IO;
using FleetTrack.Cli;
using FleetTrack.Cli.Commands;
using Xunit;

namespace FleetTrack.Tests.Cli
{
    public class CliCommandTests
    {
        private readonly FleetContext _context = new FleetContext();
        private readonly StringWriter _output = new StringWriter();

        private CommandRouter Router()
        {
            return new CommandRouter(new ICommand[]
            {
                new ListVehiclesCommand(_context),
                new AlertsCommand(_context),
                new SimulateCommand(_context),
                new ExportCommand(_context)
            });
        }

        [Fact]
        public void Help_ListsCommands_AndSucceeds()
        {
            Assert.Equal(0, Router().Run(new string[0], _output));
            Assert.Contains("export", _output.ToString());
        }

        [Fact]
        public void UnknownCommand_Fails()
        {
            Assert.Equal(1, Router().Run(new[] { "nope" }, _output));
            Assert.Contains("Unknown command 'nope'", _output.ToString());
        }

        [Fact]
        public void Vehicles_ShowsCategory()
        {
            Assert.Equal(0, Router().Run(new[] { "VEHICLES" }, _output));
            Assert.Contains("ArticulatedTruck", _output.ToString());
        }

        [Fact]
        public void Simulate_PairsTruckWithQualifiedDriver()
        {
            Assert.Equal(0, Router().Run(new[] { "simulate" }, _output));
            Assert.Contains("FT-1001 with Ava Nguyen", _output.ToString());
        }

        [Fact]
        public void ExportVehicles_ToStdout()
        {
            Assert.Equal(0, Router().Run(new[] { "export", "vehicles" }, _output));

            var text = _output.ToString();
            Assert.StartsWith("Id,LicensePlate,", text);
            Assert.Contains("FT-1004", text);
        }

        [Fact]
        public void ExportDrivers_ToFile()
        {
            var path = Path.Combine(Path.GetTempPath(), "fleettrack-" + Guid.NewGuid() + ".csv");
            try
            {
                Assert.Equal(0, Router().Run(new[] { "export", "drivers", path }, _output));
                Assert.Contains("Mia Rossi", File.ReadAllText(path));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void Export_InvalidArguments_Fail()
        {
            Assert.Equal(1, Router().Run(new[] { "export" }, _output));
            Assert.Equal(1, Router().Run(new[] { "export", "trips" }, _output));
            Assert.Equal(1, Router().Run(new[] { "export", "vehicles", Path.Combine("/nonexistent-dir", "x.csv") }, _output));
        }
    }
}

using System;
using System.IO;
using System.Linq;
using FleetTrack.Infrastructure.Export;

namespace FleetTrack.Cli.Commands
{
    public class ExportCommand : ICommand
    {
        private readonly FleetContext _context;

        public ExportCommand(FleetContext context)
        {
            _context = context;
        }

        public string Name => "export";

        public string Description => "Export vehicles|drivers as CSV to stdout or [file]";

        public int Execute(string[] args, TextWriter output)
        {
            if (args.Length == 0)
            {
                output.WriteLine("Usage: export vehicles|drivers [file]");
                return 1;
            }

            string csv;
            switch (args[0].ToLowerInvariant())
            {
                case "vehicles":
                    csv = CsvExporter.ExportVehicles(_context.Vehicles.GetAll().OrderBy(v => v.LicensePlate));
                    break;
                case "drivers":
                    csv = CsvExporter.ExportDrivers(_context.Drivers.GetAll().OrderBy(d => d.FullName));
                    break;
                default:
                    output.WriteLine("Unknown export '{0}'. Use 'vehicles' or 'drivers'.", args[0]);
                    return 1;
            }

            if (args.Length == 1)
            {
                output.Write(csv);
                return 0;
            }

            try
            {
                File.WriteAllText(args[1], csv);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                output.WriteLine("Could not write {0}: {1}", args[1], ex.Message);
                return 1;
            }

            output.WriteLine("Wrote {0} to {1}.", args[0].ToLowerInvariant(), args[1]);
            return 0;
        }
    }
}

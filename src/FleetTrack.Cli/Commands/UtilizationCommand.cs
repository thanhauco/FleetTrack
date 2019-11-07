using System;
using System.IO;
using FleetTrack.Core.Common;

namespace FleetTrack.Cli.Commands
{
    public class UtilizationCommand : ICommand
    {
        private readonly FleetContext _context;

        public UtilizationCommand(FleetContext context)
        {
            _context = context;
        }

        public string Name => "utilization";

        public string Description => "Vehicle utilization for the last N days (default 30)";

        public int Execute(string[] args, TextWriter output)
        {
            var days = 30;
            if (args.Length > 0 && !int.TryParse(args[0], out days))
            {
                output.WriteLine("Days must be a number.");
                return 1;
            }

            var to = _context.Clock.UtcNow;
            var rows = _context.Utilization.Build(new DateRange(to.AddDays(-days), to));

            output.WriteLine("{0,-10} {1,6} {2,10} {3,8} {4,7}", "Plate", "Trips", "Km", "Hours", "Util %");
            foreach (var row in rows)
            {
                output.WriteLine("{0,-10} {1,6} {2,10:N1} {3,8:N1} {4,7:N2}", row.LicensePlate, row.TripCount, row.DistanceKm, row.DrivingHours, row.UtilizationPercent);
            }

            return 0;
        }
    }
}

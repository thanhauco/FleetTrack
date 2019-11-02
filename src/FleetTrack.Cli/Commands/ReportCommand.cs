using System;
using System.IO;
using System.Linq;

namespace FleetTrack.Cli.Commands
{
    public class ReportCommand : ICommand
    {
        private readonly FleetContext _context;

        public ReportCommand(FleetContext context)
        {
            _context = context;
        }

        public string Name => "report";

        public string Description => "Cost report per vehicle for the last N days (default 90)";

        public int Execute(string[] args, TextWriter output)
        {
            var days = 90;
            if (args.Length > 0 && !int.TryParse(args[0], out days))
            {
                output.WriteLine("Days must be a number.");
                return 1;
            }

            var to = _context.Clock.UtcNow;
            var from = to.AddDays(-days);

            output.WriteLine("{0,-10} {1,12} {2,12} {3,12} {4,10}", "Plate", "Maintenance", "Fuel", "Total", "Per km");
            foreach (var vehicle in _context.Vehicles.GetAll().OrderBy(v => v.LicensePlate))
            {
                var result = _context.Costs.Build(vehicle.Id, from, to);
                if (result.IsFailure)
                {
                    output.WriteLine("{0,-10} {1}", vehicle.LicensePlate, result.Error);
                    continue;
                }

                var r = result.Value;
                output.WriteLine(
                    "{0,-10} {1,12:N2} {2,12:N2} {3,12:N2} {4,10}",
                    vehicle.LicensePlate,
                    r.MaintenanceCost.Amount,
                    r.FuelCost.Amount,
                    r.TotalCost.Amount,
                    r.CostPerKm.HasValue ? r.CostPerKm.Value.ToString("0.000") : "-");
            }

            return 0;
        }
    }
}

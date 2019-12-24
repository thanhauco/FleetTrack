using System.IO;
using System.Linq;

namespace FleetTrack.Cli.Commands
{
    public class ListVehiclesCommand : ICommand
    {
        private readonly FleetContext _context;

        public ListVehiclesCommand(FleetContext context)
        {
            _context = context;
        }

        public string Name => "vehicles";

        public string Description => "List all vehicles with status and odometer";

        public int Execute(string[] args, TextWriter output)
        {
            output.WriteLine("{0,-10} {1,-10} {2,-10} {3,-17} {4,-14} {5,10}", "Plate", "Make", "Model", "Category", "Status", "Odometer");

            foreach (var v in _context.Vehicles.GetAll().OrderBy(v => v.LicensePlate))
            {
                output.WriteLine("{0,-10} {1,-10} {2,-10} {3,-17} {4,-14} {5,10:N0}", v.LicensePlate, v.Make, v.Model, v.Category, v.Status, v.OdometerKm);
            }

            return 0;
        }
    }
}

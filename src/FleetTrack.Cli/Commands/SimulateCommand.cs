using System;
using System.IO;
using System.Linq;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Geo;

namespace FleetTrack.Cli.Commands
{
    /// <summary>
    /// Plans, starts and completes a trip between two cities to exercise the whole stack.
    /// </summary>
    public class SimulateCommand : ICommand
    {
        private readonly FleetContext _context;

        public SimulateCommand(FleetContext context)
        {
            _context = context;
        }

        public string Name => "simulate";

        public string Description => "Run a sample Berlin -> Munich trip";

        public int Execute(string[] args, TextWriter output)
        {
            var vehicle = _context.Vehicles.FindByStatus(VehicleStatus.Available).FirstOrDefault();
            var driver = _context.Drivers.GetAll().FirstOrDefault(d => d.IsLicenseValid(_context.Clock.UtcNow));
            if (vehicle == null || driver == null)
            {
                output.WriteLine("No available vehicle or valid driver.");
                return 1;
            }

            var berlin = new GeoPoint(52.5200, 13.4050);
            var munich = new GeoPoint(48.1351, 11.5820);
            var km = DistanceCalculator.KilometersBetween(berlin, munich);

            var planned = _context.TripService.Plan(vehicle.Id, driver.Id, berlin, munich, _context.Clock.UtcNow);
            if (planned.IsFailure)
            {
                output.WriteLine("Plan failed: {0}", planned.Error);
                return 1;
            }

            output.WriteLine("Planned trip for {0} with {1} ({2:F0} km).", vehicle.LicensePlate, driver.FullName, km);

            var started = _context.TripService.Start(planned.Value.Id);
            if (started.IsFailure)
            {
                output.WriteLine("Start failed: {0}", started.Error);
                return 1;
            }

            var completed = _context.TripService.Complete(planned.Value.Id, vehicle.OdometerKm + Math.Round(km));
            if (completed.IsFailure)
            {
                output.WriteLine("Complete failed: {0}", completed.Error);
                return 1;
            }

            output.WriteLine("Trip completed, vehicle odometer now {0:N0} km.", _context.Vehicles.Get(vehicle.Id).OdometerKm);
            return 0;
        }
    }
}

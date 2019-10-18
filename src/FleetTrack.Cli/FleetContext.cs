using FleetTrack.Core.Common;
using FleetTrack.Core.Events;
using FleetTrack.Core.Services;
using FleetTrack.Infrastructure.Persistence;
using FleetTrack.Infrastructure.Seed;

namespace FleetTrack.Cli
{
    /// <summary>
    /// Wires the object graph by hand; the CLI keeps everything in memory and seeds demo data.
    /// </summary>
    public class FleetContext
    {
        public FleetContext()
        {
            Clock = new SystemClock();
            Events = new InProcessEventBus();

            Vehicles = new InMemoryVehicleRepository();
            Drivers = new InMemoryDriverRepository();
            Trips = new InMemoryTripRepository();
            Maintenance = new InMemoryMaintenanceRepository();
            FuelLogs = new InMemoryFuelLogRepository();

            Scheduler = new MaintenanceScheduler();
            Alerts = new AlertService(Vehicles, Drivers, Scheduler, Clock);
            Costs = new CostReportService(Maintenance, FuelLogs, Vehicles);
            Utilization = new UtilizationReportService(Trips, Vehicles);
            TripService = new TripService(Trips, Vehicles, Drivers, Clock, Events);

            new DemoDataSeeder(Vehicles, Drivers, Maintenance, FuelLogs, Clock).Seed();
        }

        public IClock Clock { get; }

        public IEventBus Events { get; }

        public InMemoryVehicleRepository Vehicles { get; }

        public InMemoryDriverRepository Drivers { get; }

        public InMemoryTripRepository Trips { get; }

        public InMemoryMaintenanceRepository Maintenance { get; }

        public InMemoryFuelLogRepository FuelLogs { get; }

        public MaintenanceScheduler Scheduler { get; }

        public AlertService Alerts { get; }

        public CostReportService Costs { get; }

        public UtilizationReportService Utilization { get; }

        public TripService TripService { get; }
    }
}

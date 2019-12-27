using System;
using FleetTrack.Cli.Commands;

namespace FleetTrack.Cli
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var context = new FleetContext();

            var router = new CommandRouter(new ICommand[]
            {
                new ListVehiclesCommand(context),
                new AlertsCommand(context),
                new ReportCommand(context),
                new SimulateCommand(context),
                new UtilizationCommand(context),
                new ExportCommand(context)
            });

            return router.Run(args, Console.Out);
        }
    }
}

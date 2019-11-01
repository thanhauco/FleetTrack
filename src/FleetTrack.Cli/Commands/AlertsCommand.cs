using System.IO;

namespace FleetTrack.Cli.Commands
{
    public class AlertsCommand : ICommand
    {
        private readonly FleetContext _context;

        public AlertsCommand(FleetContext context)
        {
            _context = context;
        }

        public string Name => "alerts";

        public string Description => "Show maintenance and licence alerts";

        public int Execute(string[] args, TextWriter output)
        {
            var alerts = _context.Alerts.GetAlerts();
            if (alerts.Count == 0)
            {
                output.WriteLine("No alerts.");
                return 0;
            }

            foreach (var alert in alerts)
            {
                output.WriteLine("[{0}] {1}", alert.Severity.ToString().ToUpperInvariant(), alert.Message);
            }

            return 0;
        }
    }
}

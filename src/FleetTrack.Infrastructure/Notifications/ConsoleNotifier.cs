using System;
using System.IO;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Infrastructure.Notifications
{
    public class ConsoleNotifier : INotifier
    {
        private readonly TextWriter _writer;

        public ConsoleNotifier(TextWriter writer = null)
        {
            _writer = writer ?? Console.Out;
        }

        public void Notify(string subject, string message)
        {
            _writer.WriteLine("[NOTIFY] {0}: {1}", subject, message);
        }
    }
}

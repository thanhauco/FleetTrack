using System.IO;
using FleetTrack.Infrastructure.Notifications;
using Xunit;

namespace FleetTrack.Tests.Infrastructure
{
    public class ConsoleNotifierTests
    {
        [Fact]
        public void Notify_WritesToProvidedWriter()
        {
            var writer = new StringWriter();
            var notifier = new ConsoleNotifier(writer);

            notifier.Notify("Subject", "Body");

            Assert.Contains("[NOTIFY] Subject: Body", writer.ToString());
        }
    }
}

using System.IO;

namespace FleetTrack.Cli.Commands
{
    public interface ICommand
    {
        string Name { get; }

        string Description { get; }

        int Execute(string[] args, TextWriter output);
    }
}

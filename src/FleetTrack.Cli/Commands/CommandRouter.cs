using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FleetTrack.Cli.Commands
{
    public class CommandRouter
    {
        private readonly Dictionary<string, ICommand> _commands;

        public CommandRouter(IEnumerable<ICommand> commands)
        {
            _commands = commands.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
        }

        public int Run(string[] args, TextWriter output)
        {
            if (args.Length == 0 || args[0] == "help" || args[0] == "--help")
            {
                PrintHelp(output);
                return 0;
            }

            ICommand command;
            if (!_commands.TryGetValue(args[0], out command))
            {
                output.WriteLine("Unknown command '{0}'.", args[0]);
                PrintHelp(output);
                return 1;
            }

            return command.Execute(args.Skip(1).ToArray(), output);
        }

        private void PrintHelp(TextWriter output)
        {
            output.WriteLine("Usage: fleettrack <command> [options]");
            output.WriteLine();
            output.WriteLine("Commands:");
            foreach (var command in _commands.Values.OrderBy(c => c.Name))
            {
                output.WriteLine("  {0,-12} {1}", command.Name, command.Description);
            }
        }
    }
}

using System.Collections.Generic;

namespace Galactic1.Dev
{
    public class HelpCommand : IDevCommand
    {
        private DevConsole console;
        private Dictionary<string, IDevCommand> commands;

        public HelpCommand(DevConsole console, Dictionary<string, IDevCommand> commands)
        {
            this.console = console;
            this.commands = commands;
        }

        public string Name => "help";
        public string Description => "Lists all available commands";

        public void Execute(string[] args)
        {
            foreach (var cmd in commands.Values)
                console.Log($"{cmd.Name} - {cmd.Description}");
        }
    }

}
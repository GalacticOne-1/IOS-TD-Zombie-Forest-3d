using UnityEngine;

namespace Galactic1.Dev
{
    public class TimescaleCommand : IDevCommand
    {
        private DevConsole console;
        public TimescaleCommand(DevConsole console) => this.console = console;

        public string Name => "timescale";
        public string Description => "Sets game speed: timescale <value>";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                console.Log($"Current timescale: {Time.timeScale}");
                return;
            }

            if (float.TryParse(args[0], out var value))
            {
                Time.timeScale = value;
                console.Log($"Timescale set to {value}");
            }
            else
            {
                console.Log("Invalid value");
            }
        }
    }

}
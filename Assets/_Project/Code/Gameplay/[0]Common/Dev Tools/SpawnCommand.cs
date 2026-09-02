using UnityEngine;

namespace Galactic1.Dev
{
    public class SpawnCommand : IDevCommand
    {
        private DevSpawner spawner;
        private DevConsole console;

        public SpawnCommand(DevConsole console, DevSpawner spawner)
        {
            this.console = console;
            this.spawner = spawner;
        }

        public string Name => "spawn";
        public string Description => "Spawns a prefab: spawn <PrefabName>";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                console.Log("Usage: spawn <PrefabName>");
                return;
            }

            string prefabName = args[0];
            var prefab = spawner.entityConfigs.Find(id => id == prefabName);

            if (prefab != null)
            {
                spawner.Spawn(prefabName, Vector3.zero);
                console.Log($"Spawned {prefabName} at (0,0,0)");
            }
            else
            {
                console.Log($"Prefab '{prefabName}' not found");
            }
        }
    }

}
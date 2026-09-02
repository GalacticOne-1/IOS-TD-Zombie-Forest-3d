
using Galactic1.RaidLoot.Runtime;

namespace Galactic1.RaidLoot.Scene.Lifecycle
{
    /// <summary>
    /// Runtime/View pair exposed to UI systems.
    /// </summary>
    public readonly struct LootContainerSceneData
    {
        public readonly string RuntimeId;
        public readonly LootContainerRuntime Runtime;
        public readonly LootContainerView View;

        public LootContainerSceneData(
            LootContainerRuntime runtime,
            LootContainerView view)
        {
            RuntimeId = runtime.Id;
            Runtime = runtime;
            View = view;
        }
    }
}
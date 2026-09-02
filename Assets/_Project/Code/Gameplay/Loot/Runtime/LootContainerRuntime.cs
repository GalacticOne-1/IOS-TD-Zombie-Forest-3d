
using System;
using System.Collections.Generic;
using Galactic1.RaidLoot.Definition;
using Galactic1.RaidLoot.Enums;

namespace Galactic1.RaidLoot.Runtime
{
    /// <summary>
    /// Источник истины для лутового контейнера во время рейда.
    /// Чистый C# — ноль зависимостей от Unity (нет Vector3, Transform, MonoBehaviour).
    /// </summary>
    public sealed class LootContainerRuntime
    {
        private readonly List<LootGenerationRecord> _generatedItems = new();

        public string Id { get; }
        public LootContainerDefinition Definition { get; }
        public ContainerState State { get; private set; } = ContainerState.Closed;
        public bool IsOpened => State == ContainerState.Open;
        public bool IsInProximity { get; private set; }
        
        public float OpenProgress { get; private set; }
        public IReadOnlyList<LootGenerationRecord> GeneratedItems => _generatedItems;

        public event Action<ContainerState> OnStateChanged;
        public event Action<bool> OnProximityChanged;
        public event Action<float> OnOpenProgressChanged;

        public LootContainerRuntime(LootContainerDefinition definition)
        {
            Id = Guid.NewGuid().ToString();
            Definition = definition;
        }

        public void SetState(ContainerState state)
        {
            State = state;
            OnStateChanged?.Invoke(state);
        }
        
        // ── Proximity — gameplay signal, not rendering logic ──────────────────

        /// <summary>
        /// Called by ProximityTrigger on enter/exit.
        /// Guard: no-op if value hasn't changed.
        /// </summary>
        public void SetInProximity(bool inProximity)
        {
            if (IsInProximity == inProximity) 
                return;
            
            IsInProximity = inProximity;
            OnProximityChanged?.Invoke(inProximity);
        }
        
        /// <summary>Called by ProximityTrigger every frame during opening countdown.</summary>
        public void SetOpenProgress(float progress)
        {
            OpenProgress = progress;
            OnOpenProgressChanged?.Invoke(progress);
        }

        public void StoreGeneratedItems(IEnumerable<LootGenerationRecord> items)
        {
            _generatedItems.Clear();
            _generatedItems.AddRange(items);
        }

        public bool TryRemoveItem(LootGenerationRecord record)
        {
            var removed = _generatedItems.Remove(record);
            if (removed && _generatedItems.Count == 0)
                SetState(ContainerState.FullyLooted);
            return removed;
        }
    }
}
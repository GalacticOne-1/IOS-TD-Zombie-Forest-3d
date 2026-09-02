using System;
using Galactic1.Code.Gameplay.Units.Stats;
using R3;

namespace Galactic1.Code.UI.Units.Presentation
{
    /// <summary>
    /// Только чтение. UI не знает про Runtime.
    /// </summary>
    public interface IReadOnlyStatsView
    {
        float MaxHP { get; }

        event Action<StatChangedEvent, bool> OnStatChanged;
        void Dispose();
        
        ReactiveProperty<float> Get(StatId statId);
        void PushAllStats();
    }
}
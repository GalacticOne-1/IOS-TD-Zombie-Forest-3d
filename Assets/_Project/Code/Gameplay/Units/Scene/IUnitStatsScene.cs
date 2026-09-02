
using System;
using R3;

namespace Galactic1.Code.UI.Units.Presentation
{
    public interface IUnitStatsScene
    {
        bool IsDead { get; }
        float MaxHP { get; }
        event Action OnDeath;
        
        ReactiveProperty<float> Get(StatId type);
        void ModifyStat(StatId type, float delta);
        void Dispose();
    }
}
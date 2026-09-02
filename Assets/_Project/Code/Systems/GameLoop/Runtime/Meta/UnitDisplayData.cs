
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.UI.Units.Presentation;
using UnityEngine;

namespace Galactic1.Code.UI.Units
{
    /// <summary>
    /// UI Data для одного юнита
    /// ------------------------------------------------------------
    /// - Агрегирует Stats, Inventory и EquipmentPresentation
    /// - Владеет подписками на изменения
    /// - Сцена обращается только к этому объекту
    /// </summary>
    public sealed class UnitDisplayData
    {
        public string Id { get; }
        public string ArchetypeId { get; }
        public string DisplayName { get; }
        //public GameObject Prefab { get; }

        // Основные presentation-свойства для UI
        public IReadOnlyStatsView Stats { get; private set; }
        

        public UnitDisplayData(IUnitRuntime runtimeUnit)
        {
            Id = runtimeUnit.Id;
            ArchetypeId = runtimeUnit.ArchetypeId;
            DisplayName = runtimeUnit.DisplayName;
            //Prefab = runtimeUnit.GetPrefab();

            // Stats — оборачиваем адаптером Presentation
            Stats = new UnitStatsPresentationAdapter(runtimeUnit.Stats);
        }
        
        public void Rebind(IUnitRuntime newRuntime)
        {
            Stats.Dispose();         // отписываемся от старого
            Stats = new UnitStatsPresentationAdapter(newRuntime.Stats);
            Stats.PushAllStats();    // форсируем обновление UI
        }

        public void Dispose()
        {
            Stats.Dispose();
        }

    }
}
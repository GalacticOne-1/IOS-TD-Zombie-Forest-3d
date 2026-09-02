using System;
using UnityEngine;

namespace Galactic1.Core.UI
{
    
    /// <summary>
    /// Должен висеть на UI панели где есть статы для прокси, что бы само обновлялось
    /// (health, epxerience, hunger, combat stats, etc)
    /// </summary>
    public class UIStatsPanel : MonoBehaviour
    {
        [field: SerializeField] public bool IsDragon { get; private set; }
        [field: SerializeField] public SlotGroup[] SlotGroups  { get; private set; }
        
        [Serializable]
        public struct SlotGroup
        {
            public StatsDrawRules drawRules;
            public StatSlotUI[] statSlots;
        }

        public void Initialize()
        {
            // передаем в каждый слот правило отрисовки статы
            foreach (var group in SlotGroups)
            {
                foreach (var slot in group.statSlots)
                {
                    slot.DrawRules = group.drawRules;
                }
            }
        }
    }
}
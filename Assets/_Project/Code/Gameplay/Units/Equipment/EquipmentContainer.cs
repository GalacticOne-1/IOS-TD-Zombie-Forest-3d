using Galactic1.Code.Gameplay.Equipment_Preview;
using Galactic1.Code.Gameplay.Equipment;
using UnityEngine;

namespace Galactic1.Code.UI.Equipment
{
    /// <summary>
    /// Mono-мост между юнитом, визуалом и runtime-логикой.
    /// Больше НЕ содержит логики экипировки.
    /// </summary>
    public sealed class EquipmentContainer : MonoBehaviour
    {
        [SerializeField] private EquipmentVisualController visuals;


        public void BindSource(IEquipmentStatsProvider presentation, IEquipmentVisualHandler handler)
        {
            visuals.Bind(presentation, handler);
        }

    }
}
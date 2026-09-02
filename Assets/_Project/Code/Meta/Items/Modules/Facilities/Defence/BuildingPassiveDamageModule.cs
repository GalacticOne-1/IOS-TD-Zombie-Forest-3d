
using Galactic1.Code.Gameplay.AoE;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Пассивный источник урона: колья, шипы, электрозаграждения, огненные ловушки.
    /// Не стреляет, не знает КАК наносится урон — это описывает TemporalEffectConfig
    /// (тот же конфиг, что используют зональные гранаты).
    /// Модуль отвечает только за то, КОГДА и ПРИ КАКИХ условиях эффект применяется.
    /// </summary>
    [System.Serializable]
    public class BuildingPassiveDamageModule : ItemModule
    {
        [Header("Effect")]
        [Tooltip("Описывает урон, радиус, тики и доп. эффекты (стан, замедление). Тот же тип, что используют зональные гранаты.")]
        [SerializeField]
        private AreaEffectConfig effectConfig;

        public AreaEffectConfig EffectConfig => effectConfig;
    }

}
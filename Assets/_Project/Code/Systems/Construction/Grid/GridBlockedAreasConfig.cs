using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Grid
{
    /// <summary>
    /// ScriptableObject конфигурации статических заблокированных зон сетки.
    ///
    /// Хранит список прямоугольных областей (рок, декор, зарезервированное
    /// геймплейное пространство и т.д.), где строительство запрещено.
    /// Редактируется левел-дизайнерами.
    /// </summary>
    [CreateAssetMenu(
        fileName = "GridBlockedAreasConfig",
        menuName = "Game Configs/Construction/Grid Blocked Areas Config")]
    public class GridBlockedAreasConfig : ScriptableObject
    {
        [Header("Blocked Areas")]
        [SerializeField] private List<BlockedGridArea> blockedAreas = new();

        public List<BlockedGridArea> BlockedAreas => blockedAreas;
        
        
        /// <summary>
        /// Вызывается при правке ассета в инспекторе.
        /// Используется только для dev-удобства (live-обновление сервиса/гизмо).
        /// </summary>
        public event Action Changed;

#if UNITY_EDITOR
        private void OnValidate()
        {
            Changed?.Invoke();
        }
#endif
    }
}
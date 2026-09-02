
using System;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Configs.WorldMap
{
    /// <summary>
    /// Конфиг стилей маркеров локаций на глобальной карте.
    /// Определяет иконку и цвет для каждого типа локации.
    /// Используется LocationMarker (UI only).
    /// </summary>
    [CreateAssetMenu(
        fileName = "ManagerUIStyleConfig",
        menuName = "Game Configs/Style/Camp Style Config"
    )]
    public class ManagerUIStyleConfig : StyleConfigBase
    {
        [SerializeField] private CMenuTabIcon[] tabIcons;



        /// <summary>
        /// Иконка для вкладок
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Sprite GetTabIcon(UIScreenId id)
        {
            var l = tabIcons.Length;
            for (int i = 0; i < l; i++)
            {
                if (tabIcons[i].key == id)
                    return tabIcons[i].icon;
            }

            return null;
        }
    }

    [Serializable]
    public struct CMenuTabIcon
    {
        public UIScreenId key;
        public Sprite icon;
    }
}
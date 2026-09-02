using System;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Game.Meta.Economy
{
    /// <summary>
    /// Описание виртуальной валюты.
    ///
    /// Используется:
    /// - UI
    /// - магазином
    /// - системой наград
    /// - уведомлениями
    /// - отображением стоимости
    /// </summary>
    [CreateAssetMenu(
        fileName = "CurrencyConfig",
        menuName = "Game Configs/Economy/Currency Config")]
    public sealed class CurrencyConfig : ScriptableObject
    {
        #region Identity

        [Header("Identity")]
        [SerializeField] private RuntimeId id;

        [SerializeField]
        private int version = 1;

        public RuntimeId Id => id;
        public int Version => version;

        #endregion

        #region Header

        [Header("Presentation")]
        [SerializeField]
        private HeaderData header;

        [Serializable]
        public struct HeaderData
        {
            public string titleLid;

            [TextArea]
            public string descriptionLid;

            public int order;

            [Space]
            public Sprite icon;

            public float sizeUI;
            public Vector2 iconOffset;
        }

        public HeaderData Header => header;

        #endregion

        #region Currency

        [Header("Currency")]
        [SerializeField]
        private EBankResourceType type;

        [SerializeField]
        private int sortOrder;

        public EBankResourceType Type => type;
        public int SortOrder => sortOrder;

        #endregion


    }
}
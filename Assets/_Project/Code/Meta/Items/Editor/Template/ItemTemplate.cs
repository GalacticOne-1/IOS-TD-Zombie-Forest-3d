using System;
using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.RaidLoot.Authoring;
using Galactic1.UI.CharacterPreview;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Template used to quickly create ItemConfig with predefined modules and settings.
    /// Used only by editor tools.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EditorItemTemplate",
        menuName = "Game Configs/Inventory/Editor Item Template")]
    public class ItemTemplate : ScriptableObject
    {
        [Header("Template Info")]
        [SerializeField] private string templateId;
        [SerializeField] private string displayName;
        [SerializeField] private string menuName;
        [SerializeField] private UIPreviewConfig previewConfig;

        [Header("Default Classification")]
        [SerializeField] private ItemCategory category;
        [SerializeField] private LootEconomyCategory economyCategory;
        [SerializeField] private ItemLabel label;
        [SerializeField] private ItemRarity rarity;
        [SerializeField] ItemSortCategory sortCategory;
        [SerializeField] private int maxStack = 1;

        [Header("Default Economy")]
        [SerializeField] private int buyPrice;
        [SerializeField] private int sellPrice;

        [Header("Default Physical")]
        [SerializeField] private float weight;
        [SerializeField] private float volume;

        [Header("Modules")]
        [SerializeReference]
        private List<ItemModule> modules = new();

        public string TemplateId => templateId;
        public string DisplayName => displayName;

        public string MenuName => menuName;
        public UIPreviewConfig PreviewConfig => previewConfig;
        
        

        public ItemCategory Category => category;

        public LootEconomyCategory EconomyCategory => economyCategory;

        public ItemLabel Label => label;
        public ItemRarity Rarity => rarity;

        public ItemSortCategory SortCategory => sortCategory;

        public int MaxStack => maxStack;

        public int BuyPrice => buyPrice;
        public int SellPrice => sellPrice;

        public float Weight => weight;
        public float Volume => volume;

        public IReadOnlyList<ItemModule> Modules => modules;
    }
}
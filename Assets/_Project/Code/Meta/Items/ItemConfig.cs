using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Items;
using Galactic1.Code.Systems.Economy;
using Galactic1.Configs;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Stats;
using Galactic1.PoolObject;
using Galactic1.RaidLoot.Authoring;
using Galactic1.Structure;
using Galactic1.UI;
using Galactic1.UI.CharacterPreview;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    [CreateAssetMenu(fileName = "ItemConfig", menuName = "Game Configs/Inventory/Item Config")]
    public class ItemConfig : ScriptableObject, IItemHeaderProvider, IObjectPoolConfig
    {
        #region IDENTITY

        [Header("Identity")] [SerializeField] // [Tooltip("Stable unique key. Example: weapon.rifle.ak74")] 
        private RuntimeId id;

        [SerializeField] private int version = 1;

        public RuntimeId Id => id;
        public int Version => version;
        


        #endregion


        [SerializeField] private bool isEnabled = true;
        public bool IsEnabled => isEnabled;



        #region HEADER

        [Header("Presentation")] [SerializeField]
        private HeaderData header;

        [Serializable]
        public struct HeaderData
        {
            public string titleLid;
            [TextArea] public string descriptionLid;

            public int order;

            [Space] public Sprite icon;
            public float sizeUI;
            public Vector2 iconOffset;
        }

        public HeaderData Header => header;

        #endregion

        #region PREVIEW

        [SerializeField] private UIPreviewConfig previewConfig;
        public UIPreviewConfig PreviewConfig => previewConfig;

        #endregion

        #region CLASSIFICATION

        [Header("Classification")] [SerializeField]
        private ClassificationData classification;

        [Serializable]
        public struct ClassificationData
        {
            public ItemCategory category;
            public LootEconomyCategory economyCategory;
            public Tier tier;
            public ItemFlags flag;
            public ItemLabel itemLabel;
            public ItemRarity rarity;

            public int maxStack;

            public ItemSortCategory sortCategory;
            public int sortPriority;
        }

        public ClassificationData Classification => classification;
        
        public bool IsStackable => classification.maxStack > 1;

        #endregion

        
        #region Recruitment

        [Header("Recruitment Access")] 
        [SerializeField] private RecruitAccess recruitAccess;

        public RecruitAccess RecruitAccess => recruitAccess;

        #endregion
        

        #region ECONOMY

        [Header("Economy")] 
        [SerializeField] private EconomyData economy;

        [Serializable]
        public struct EconomyData
        {
            public int buyPrice;
            public int sellPrice;
            public int scrapValue;

            public bool canBeSold;
            public bool canBeDestroyed;
        }

        public EconomyData Economy => economy;

        #endregion

        #region PHYSICAL

        [Header("Physical")] 
        [SerializeField] private PhysicalData physical;

        [Serializable]
        public struct PhysicalData
        {
            public float weight;
            public float volume;

            public bool usesDurability;
            public int maxDurability;
            public DurabilityLossType durabilityLossType;

            // починка в обновлении будет   
            public bool canBeRepaired;
        }

        public PhysicalData Physical => physical;

        #endregion

        #region REQUIREMENTS

        [Header("Requirements")] [SerializeField]
        private Requirement requirements;

        [Serializable]
        public struct Requirement
        {
            public Tier tier;
            public List<RequirementData> statRequirements;
        }

        public Requirement Requirements => requirements;

        #endregion

        #region TAGS

        [Header("Tags")] 
        [SerializeField] private List<ItemTag> tags = new();

        public IReadOnlyList<ItemTag> Tags => tags;

        #endregion
        
        #region CRAFTING

        [Header("Crafting")]
        [SerializeField] private bool isCraftable;
        [SerializeField] private List<CraftRecipeConfig> recipes = new();

        public bool IsCraftable => isCraftable;
        public IReadOnlyList<CraftRecipeConfig> Recipes => recipes;
        
        public void AddRecipe(CraftRecipeConfig recipe) 
            => recipes.Add(recipe);
        
        public void RemoveRecipe(int index)
            => recipes.RemoveAt(index);

        #endregion


        #region BASE MODIFIERS

        [Header("Base Modifiers (Always Applied When Equipped/Used)")] [SerializeField]
        private List<StatModifier> baseModifiers = new();

        public IReadOnlyList<StatModifier> BaseModifiers => baseModifiers;

        #endregion

        #region WORLD

        [Header("World Representation")] 
        [SerializeField] private string prefabName;
        [SerializeField] private string prefabPath;
        //[SerializeField] private GameObject worldPrefab;
        //[SerializeField] private GameObject droppedPrefab;

        
        public string PrefabName => prefabName;
        public string PrefabPath => prefabPath + prefabName;
        //public string GhostPrefabPath => ghostPrefabPath;
        //public GameObject WorldPrefab => worldPrefab;
        //public GameObject DroppedPrefab => droppedPrefab;

        
        [SerializeField] private ObjectPoolParam objectPoolParam;
        public ObjectPoolParam ObjectPoolParam => objectPoolParam;

        
        #endregion
        
        
        
        #region MODULES

        [SerializeReference]
        private List<ItemModule> modules = new();
        public IReadOnlyList<ItemModule> Modules => modules;
        
        private ModuleRegistry registry;

        private void EnsureCache()
        {
            if (registry != null)
                return;

            registry = new ModuleRegistry();

            foreach (var module in modules)
            {
                if (module == null)
                    continue;

                module.OnItemCreated(this);

                registry.Register(module);
            }
        }

        public T GetModule<T>() where T : ItemModule
        {
            EnsureCache();
            return registry.Get<T>();
        }

        public bool HasModule<T>() where T : ItemModule
        {
            EnsureCache();
            return registry.Has<T>();
        }
        
        #endregion


        #region Shortcuts
        
        public ActionModule Action => GetModule<ActionModule>();
        public LootModule LootModule => GetModule<LootModule>();
        
        public ResourceModule Resource => GetModule<ResourceModule>();
        
        public UseModule Use => GetModule<UseModule>();
        public WeaponModule Weapon => GetModule<WeaponModule>();
        public EquipmentModule Equipment => GetModule<EquipmentModule>();
        public AmmoModule Ammo => GetModule<AmmoModule>();
        public UpgradeModule Upgrade => GetModule<UpgradeModule>();
        
        public VehicleModule Vehicle => GetModule<VehicleModule>();
        public VehicleEquipmentModule VehicleEquipment => GetModule<VehicleEquipmentModule>();
        public VehicleModuleBase VehicleModule => GetModule<VehicleModuleBase>();
        
        
        public BlueprintModule Blueprint => GetModule<BlueprintModule>();
        
        
        // facilities
        public MainContainerModule MainContainer => GetModule<MainContainerModule>();
        public TavernModule Tavern => GetModule<TavernModule>();
        public GarageModule Garage => GetModule<GarageModule>();
        public StorageModule Storage => GetModule<StorageModule>();
        public CraftingStationModule CraftStation => GetModule<CraftingStationModule>();
        public LivingModule Living => GetModule<LivingModule>();
        
        public BuildingHealthModule BuildingHealth => GetModule<BuildingHealthModule>();
        public BuildingAttackModule BuildingAttack => GetModule<BuildingAttackModule>();
        public BuildingPassiveDamageModule BuildingPassiveDamage => GetModule<BuildingPassiveDamageModule>();

        #endregion

        
        
        
        
        
        public List<DescriptorDisplayEntry> GetDescriptors()
        {
            EnsureCache();
            
            var list = new List<DescriptorDisplayEntry>();
            
            foreach (var module in Modules)
            {
                if (module is IDescriptorProvider provider)
                    provider.CollectDescriptors(list);
            }

            return list;
        }

        /// <summary>
        /// если есть список связанных предметов 
        /// <br/>например:
        ///   <br/>- у оружия используемые боеприпасы
        ///   <br/>- у патронов всё оружие которое юзает эти патроны
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryGetLinkedItems(out StatId statId, out List<RuntimeId> result)
        {
            foreach (var module in Modules)
            {
                if (module is ILinkedItemsProvider provider)
                {
                    var res = provider.LinkedItems();
                    statId = res.Item1;
                    result = res.Item2;
                    return true;
                }
            }

            statId = 0;
            result = null;
            return false;
        }
        
        public EquipSlotType GetEquipSlot()
        {
            foreach (var module in Modules)
            {
                if (module is IEquipModule equip)
                    return equip.GetSlot();
            }

            return EquipSlotType.None;
        }


        /// <summary>
        /// Получение всего списка статов предмета
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<ItemStatEntry> GetStats()
        {
            if (HasModule<UseModule>())
                return Use.BaseStats();
            
            if (HasModule<WeaponModule>())
                return Weapon.BaseStats();
            
            if (HasModule<EquipmentModule>())
                return Equipment.BaseStats();
            
            if (HasModule<VehicleModule>())
                return Vehicle.BaseStats();

            return Array.Empty<ItemStatEntry>();
        }

        public ItemEquipType GetEquipClass()
        {
            if (HasModule<WeaponModule>())
                return Weapon.Settings.equipType;
            
            if (HasModule<EquipmentModule>())
                return Equipment.Settings.equipType;

            return ItemEquipType.None;
        }
        
        /// <summary>
        /// Подсказка для инвентаря
        /// </summary>
        /// <returns></returns>
        public TooltipItemDto BuildTooltip()
        {
            var data = new TooltipItemDto();

            // если модуль имеет что-то для подсказки то он добавит это в TooltipData
            foreach (var module in Modules)
                module.BuildTooltip(ref data);


            return data;
        }

        /// <summary>
        /// Сравнение одной статы
        /// </summary>
        /// <param name="toCompare"></param>
        /// <returns></returns>
        public CompareStat StatCompare(StatId toCompare, float value)
        {
            CompareStat result;
            foreach (var module in Modules)
            {
                result = module.StatCompare(toCompare, value);
                if (result != CompareStat.Fail)
                    return result;
            }

            return CompareStat.Fail;
        }
        
        
        


#if UNITY_EDITOR
        private void OnValidate()
        {
            //itemKey = name;
            registry = null;
            
            ValidateStacking();
            ValidateDurability();
        }


        private void ValidateStacking()
        {
            if (classification.maxStack <= 0)
                classification.maxStack = 1;
        }

        private void ValidateDurability()
        {
            if (!physical.usesDurability)
                physical.maxDurability = 0;

            if (physical.maxDurability < 0)
                physical.maxDurability = 0;
        }
#endif
        
        
    }

    
    
    public interface IEquipModule
    {
        EquipSlotType GetSlot();
    }
    
    
    
    
    [Flags]
    public enum ItemFlags
    {
        None        = 0,
        QuestItem   = 1 << 0,
        Unique      = 1 << 1,
        CannotDrop  = 1 << 2,
        CannotStore = 1 << 3,
        CannotSell  = 1 << 4,
    
        // доступность отображения
        HideInConstruct  = 1 << 5,
        HideInGarage  = 1 << 6,
        HideInShop    = 1 << 7,
        HideInCraft   = 1 << 8,
    }
    
    public enum DurabilityLossType
    {
        None,
        OnUse,
        OnHit,
        OnDamageTaken,
        PerSecond
    }
}
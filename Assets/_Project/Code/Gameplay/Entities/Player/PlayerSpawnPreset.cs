
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;
using UnityEngine;

namespace Galactic1.Gameplay.Player
{
    /*
     * PlayerSpawnPreset — аналог PlayerConfig/SpawnPreset в LDoE.
     * Этот SO содержит все данные для спавна игрока:
     * - позиция появления
     * - статы
     * - одежда/оружие
     * - стартовый инвентарь
     * - состояние: пешком, на драконе, в стелсе.
     */
    [CreateAssetMenu(fileName = "PlayerSpawnPreset", menuName = "Game Configs/Player/Player Spawn Preset")]
    public class PlayerSpawnPreset : ScriptableObject
    {
        [field: SerializeField] public string ConfigId { get; private set; }
        public string Id
        {
            get => ConfigId;
            set => ConfigId = value;
        }


        [Header("Player")] 
        [SerializeField] private PlayerCharacterConfig characterConfig;
        [SerializeField] private CharacterStatsBase playerStatsBase;

        [Header("Equipment")] 
        [SerializeField] private ItemConfig startingWeapon;

        [SerializeField] private ItemConfig[] startingArmor;

        [Header("Inventory")] 
        [SerializeField] private ItemConfig[] startingItems;

        [Header("Stats")] 
        [SerializeField] private int level = 1;

        [SerializeField] private int health = 100;
        [SerializeField] private int energy = 100;


        public PlayerLoadData GetData()
        {
            return new PlayerLoadData()
            {
                CharacterConfig = characterConfig,
                PlayerStatsBase = playerStatsBase,
                weapon = startingWeapon,
                armor = startingArmor,
                inventoryItems = startingItems,
                level = level,
                health = health,
                energy = energy,
            };
        }
    }

    /// <summary>
    /// Runtime container for player load data extracted from ScriptableObject.
    /// </summary>
    public struct PlayerLoadData
    {
        public PlayerCharacterConfig CharacterConfig;
        public CharacterStatsBase PlayerStatsBase;
        public IUnitRuntime UnitRuntime;
        public IInventoryResourcesPort InventoryPort;
        public WeaponAnimLibrary AnimLibrary;

        public ItemConfig weapon;
        public ItemConfig[] armor;
        public ItemConfig[] inventoryItems;

        public int level;
        public int health;
        public int energy;
    }

    public struct EnemyLoadData
    {
        public EnemyRuntime Runtime;
    }
}
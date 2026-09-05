using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.RaidLoot.Authoring
{
    [CreateAssetMenu(
        fileName = "LocationLootProfileConfig",
        menuName = "Game Configs/Loot/Location Loot Profile Config")]
    public class LocationLootProfileConfig : ScriptableObject
    {
        [Header("Category Multipliers")]
        [Tooltip("Default-правило для всех предметов данной экономической категории.")]
        [FormerlySerializedAs("multipliers")]
        [SerializeField]
        private LootMultiplierEntry[] _categoryMultipliers;

        [Header("Item Overrides")]
        [Tooltip("Location-specific исключение для одного предмета. Приоритетнее категории.")]
        [SerializeField]
        private LootItemMultiplierEntry[] _itemMultipliers;

        public LootMultiplierEntry[] CategoryMultipliers => _categoryMultipliers;
        public LootItemMultiplierEntry[] ItemMultipliers => _itemMultipliers;

        /// <summary>
        /// Backward-compat alias. Используйте CategoryMultipliers.
        /// </summary>
        [System.Obsolete("Use CategoryMultipliers instead.")]
        public LootMultiplierEntry[] Multipliers => _categoryMultipliers;

#if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateCategoryMultipliers();
            ValidateItemMultipliers();
        }

        private void ValidateCategoryMultipliers()
        {
            if (_categoryMultipliers == null) return;

            var seen = new System.Collections.Generic.HashSet<LootEconomyCategory>();
            foreach (var entry in _categoryMultipliers)
            {
                if (!seen.Add(entry.Category))
                    Debug.LogError(
                        $"[{name}] Duplicate category multiplier for '{entry.Category}'. " +
                        "Only one entry per category is allowed.", this);

                ValidateMultiplierValue(entry.WeightMultiplier, $"Category '{entry.Category}' WeightMultiplier");
                ValidateMultiplierValue(entry.AmountMultiplier, $"Category '{entry.Category}' AmountMultiplier");
            }
        }

        private void ValidateItemMultipliers()
        {
            if (_itemMultipliers == null) return;

            var seen = new System.Collections.Generic.HashSet<ItemId>();
            foreach (var entry in _itemMultipliers)
            {
                if (entry.ItemId == null)
                {
                    Debug.LogError($"[{name}] Item override with null ItemConfig.", this);
                    continue;
                }

                if (!seen.Add(entry.ItemId))
                    Debug.LogError(
                        $"[{name}] Duplicate item override for '{entry.ItemId.name}'. " +
                        "Only one override per item is allowed — resolution is ambiguous otherwise.", this);

                ValidateMultiplierValue(entry.WeightMultiplier, $"Item '{entry.ItemId.name}' WeightMultiplier");
                ValidateMultiplierValue(entry.AmountMultiplier, $"Item '{entry.ItemId.name}' AmountMultiplier");

                if (entry.WeightMultiplier == 0f)
                    Debug.LogWarning(
                        $"[{name}] Item '{entry.ItemId.name}' has WeightMultiplier = 0 — " +
                        "it will never be selected in weighted pools.", this);
            }
        }

        private void ValidateMultiplierValue(float value, string label)
        {
            if (value < 0f)
                Debug.LogError($"[{name}] {label} is negative ({value}). Multipliers must be >= 0.", this);
            else if (float.IsNaN(value) || float.IsInfinity(value))
                Debug.LogError($"[{name}] {label} is NaN/Infinity.", this);
        }
#endif
    }
}
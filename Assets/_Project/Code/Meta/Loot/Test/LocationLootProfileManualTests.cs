#if UNITY_EDITOR
using System.Reflection;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Definition;
using UnityEditor;
using UnityEngine;

namespace Galactic1.RaidLoot.Tests
{
    /// <summary>
    /// Manual verification harness for LocationLootProfile item-override resolution.
    /// No test framework in this project — run via menu item, check Console.
    /// </summary>
    public static class LocationLootProfileManualTests
    {
        [MenuItem("Tools/Loot/Run LocationLootProfile Tests")]
        public static void RunAll()
        {
            int passed = 0, failed = 0;

            void Check(string name, bool condition)
            {
                if (condition) { passed++; Debug.Log($"[PASS] {name}"); }
                else { failed++; Debug.LogError($"[FAIL] {name}"); }
            }

            var mechParts = MakeItem("test_mech_parts", LootEconomyCategory.Mechanical);
            var steelParts = MakeItem("test_steel_parts", LootEconomyCategory.Mechanical);
            var precisionParts = MakeItem("test_precision_parts", LootEconomyCategory.Mechanical);

            // Test 1: no profile → everything 1.0
            {
                var profile = new LocationLootProfile(default, null, null);
                Check("Test1_NoProfile_Weight", profile.GetWeightMultiplier(mechParts) == 1f);
                Check("Test1_NoProfile_Amount", profile.GetAmountMultiplier(mechParts) == 1f);
            }

            // Test 2: category only
            {
                var categories = new[] { MakeCategoryEntry(LootEconomyCategory.Mechanical, 0.5f, 0.5f) };
                var profile = new LocationLootProfile(default, categories, null);
                Check("Test2_CategoryOnly_Weight", profile.GetWeightMultiplier(mechParts) == 0.5f);
                Check("Test2_CategoryOnly_Amount", profile.GetAmountMultiplier(mechParts) == 0.5f);
            }

            // Test 3: item override beats category
            {
                var categories = new[] { MakeCategoryEntry(LootEconomyCategory.Mechanical, 0.5f, 0.5f) };
                var items = new[] { MakeItemEntry(mechParts, 0.8f, 0.8f) };
                var profile = new LocationLootProfile(default, categories, items);

                Check("Test3_Override_MechParts", profile.GetAmountMultiplier(mechParts) == 0.8f);
                Check("Test3_Fallback_OtherItem", profile.GetAmountMultiplier(steelParts) == 0.5f);
                Check("Test3_Fallback_Precision", profile.GetAmountMultiplier(precisionParts) == 0.5f);
            }

            // Test 4: weight and amount independent
            {
                var items = new[] { MakeItemEntry(mechParts, 0.5f, 2.0f) };
                var profile = new LocationLootProfile(default, null, items);
                Check("Test4_WeightIndependent", profile.GetWeightMultiplier(mechParts) == 0.5f);
                Check("Test4_AmountIndependent", profile.GetAmountMultiplier(mechParts) == 2.0f);
            }

            

            // Test 8: duplicate item override is detected (logs error, keeps first)
            {
                var items = new[]
                {
                    MakeItemEntry(mechParts, 0.5f, 0.5f),
                    MakeItemEntry(mechParts, 0.9f, 0.9f)
                };
                var profile = new LocationLootProfile(default, null, items);
                Check("Test8_DuplicateKeepsFirst", profile.GetAmountMultiplier(mechParts) == 0.5f);
                // Проверь Console — там должен быть Debug.LogError про дубликат.
            }

            Debug.Log($"=== LocationLootProfile tests: {passed} passed, {failed} failed ===");

            // Cleanup — тестовые ScriptableObject-инстансы существуют только в памяти,
            // они не сохраняются как assets, поэтому явный Destroy не обязателен,
            // но чистим на всякий случай при повторных запусках в одной editor-сессии.
            Object.DestroyImmediate(mechParts);
            Object.DestroyImmediate(steelParts);
            Object.DestroyImmediate(precisionParts);
        }

        private static LootMultiplierEntry MakeCategoryEntry(
            LootEconomyCategory category, float weight, float amount)
        {
            var entry = new LootMultiplierEntry();
            SetStructField(ref entry, "_category", category);
            SetStructField(ref entry, "_weightMultiplier", weight);
            SetStructField(ref entry, "_amountMultiplier", amount);
            return entry;
        }

        private static LootItemMultiplierEntry MakeItemEntry(
            ItemConfig item, float weight, float amount)
        {
            var entry = new LootItemMultiplierEntry();
            SetStructField(ref entry, "_item", item);
            SetStructField(ref entry, "_weightMultiplier", weight);
            SetStructField(ref entry, "_amountMultiplier", amount);
            return entry;
        }

        private static ItemConfig MakeItem(string debugName, LootEconomyCategory category)
        {
            var itemId = ScriptableObject.CreateInstance<ItemId>();
            itemId.name = debugName;
            itemId.Editor_InitializeIfNeeded();

            var item = ScriptableObject.CreateInstance<ItemConfig>();
            item.name = debugName;

            SetInstanceField(item, "id", itemId);
            SetInstanceField(item, "classification", new ItemConfig.ClassificationData
            {
                economyCategory = category
            });

            return item;
        }

        private static void SetInstanceField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                Debug.LogError($"[LocationLootProfileManualTests] Field '{fieldName}' not found on {target.GetType()}");
            field?.SetValue(target, value);
        }

        private static void SetStructField<T>(ref T target, string fieldName, object value) where T : struct
        {
            var field = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError($"[LocationLootProfileManualTests] Field '{fieldName}' not found on {typeof(T)}");
                return;
            }
            object boxed = target;
            field.SetValue(boxed, value);
            target = (T)boxed;
        }
    }
}
#endif
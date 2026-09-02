using System;
using UnityEditor;
using UnityEngine;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Tools
{
    /// <summary>
    /// Utility that creates ItemConfig instances from ItemTemplate.
    /// </summary>
    public static class ItemTemplateUtility
    {
        public static ItemConfig CreateFromTemplate(ItemTemplate template, string assetPath)
        {
            var item = ScriptableObject.CreateInstance<ItemConfig>();

            var so = new SerializedObject(item);

            CopyClassification(template, so);
            CopyEconomy(template, so);
            CopyPhysical(template, so);
            CopyModules(template, so);

            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(item, assetPath);

            return item;
        }

        static void CopyClassification(ItemTemplate template, SerializedObject so)
        {
            var prop = so.FindProperty("classification");

            prop.FindPropertyRelative("category").intValue = (int)template.Category;
            prop.FindPropertyRelative("economyCategory").intValue = (int)template.EconomyCategory;
            prop.FindPropertyRelative("itemLabel").intValue = (int)template.Label;
            prop.FindPropertyRelative("rarity").intValue = (int)template.Rarity;
            prop.FindPropertyRelative("sortCategory").intValue = (int)template.SortCategory;
            prop.FindPropertyRelative("maxStack").intValue = template.MaxStack;
        }

        static void CopyEconomy(ItemTemplate template, SerializedObject so)
        {
            var prop = so.FindProperty("economy");

            prop.FindPropertyRelative("buyPrice").intValue = template.BuyPrice;
            prop.FindPropertyRelative("sellPrice").intValue = template.SellPrice;
        }

        static void CopyPhysical(ItemTemplate template, SerializedObject so)
        {
            var prop = so.FindProperty("physical");

            prop.FindPropertyRelative("weight").floatValue = template.Weight;
            prop.FindPropertyRelative("volume").floatValue = template.Volume;
        }

        static void CopyModules(ItemTemplate template, SerializedObject so)
        {
            var modulesProp = so.FindProperty("modules");

            foreach (var module in template.Modules)
            {
                modulesProp.InsertArrayElementAtIndex(modulesProp.arraySize);

                var element = modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1);

                // создаем экземпляр
                var newModule = Activator.CreateInstance(module.GetType());

                // копируем сериализованные поля
                EditorJsonUtility.FromJsonOverwrite(
                    EditorJsonUtility.ToJson(module),
                    newModule
                );

                element.managedReferenceValue = newModule;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;
using UnityEditor;
using Galactic1.Configs;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;
using Galactic1.Meta.Configs.Recruitment;
using Galactic1.Tools.Galactic1.Tools;

namespace Galactic1.Tools
{
    public class ItemManagerWindow_BottomToolbar
    {
        private ItemManagerWindow _itemManagerWindow;
        private ItemDatabase database;
        private ItemManagerSettings settings;

        public ItemManagerWindow_BottomToolbar(ItemManagerWindow itemManagerWindow)
        {
            _itemManagerWindow = itemManagerWindow;
            database = _itemManagerWindow.database;
            settings = _itemManagerWindow.settings;
        }


        public void DrawBottomToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            //if (GUILayout.Button("Create New Item")) ShowCreateMenu();
            if (GUILayout.Button("Create From Template")) ShowTemplateMenu();
            if (GUILayout.Button("Fix Missing Item IDs")) FixMissingItemIds();
            if (GUILayout.Button("Validate Items")) ValidateItems();
            if (GUILayout.Button("Validate Loot Modules")) ValidateLootModules();
            EditorGUILayout.EndHorizontal();
        }

        private void ShowCreateMenu()
        {
            var menu = new GenericMenu();

            // basic
            menu.AddItem(new GUIContent("Resource"), false, () => CreateItem(ItemCreationTemplate.Resource));
            menu.AddItem(new GUIContent("Weapon"), false, () => CreateItem(ItemCreationTemplate.Weapon));
            menu.AddItem(new GUIContent("Equipment"), false, () => CreateItem(ItemCreationTemplate.Equipment));
            menu.AddItem(new GUIContent("Vehicle Equipment"), false, () => CreateItem(ItemCreationTemplate.VehicleEquipment));
            menu.AddItem(new GUIContent("Ammo"), false, () => CreateItem(ItemCreationTemplate.Ammo));

            menu.AddSeparator("");

            // facilities
            menu.AddItem(new GUIContent("Storage"), false, () => CreateItem(ItemCreationTemplate.Storage));
            menu.AddItem(new GUIContent("Craft Station"), false, () => CreateItem(ItemCreationTemplate.CraftStation));
            menu.AddItem(new GUIContent("Living Module"), false, () => CreateItem(ItemCreationTemplate.LivingModule));
            menu.AddItem(new GUIContent("Tavern"), false, () => CreateItem(ItemCreationTemplate.Tavern));

            menu.ShowAsContext();
        }
        
        private void ShowTemplateMenu()
        {
            var menu = new GenericMenu();

            var templates = AssetDatabase.FindAssets("t:ItemTemplate");

            foreach (var guid in templates)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var template = AssetDatabase.LoadAssetAtPath<ItemTemplate>(path);

                menu.AddItem(
                    new GUIContent(template.MenuName),
                    false,
                    () =>
                    {
                        var defaultPath = GetPathForType(template.Category);

                        // *** создание папки
                        if (!AssetDatabase.IsValidFolder(defaultPath))
                        {
                            string parent = settings.basePath+"/Items";
                            string folderName = template.Category + "s";
                            AssetDatabase.CreateFolder(parent, folderName);
                        }


                        string path = EditorUtility.SaveFilePanelInProject(
                            "Create Item",
                            template.DisplayName,
                            "asset",
                            "Enter item name",
                            defaultPath);

                        if (string.IsNullOrEmpty(path)) return;

                        var item = ItemTemplateUtility.CreateFromTemplate(template, path);
                        
                        // ✅ сразу записываем guid от имени конфига
                        string itemName = System.IO.Path.GetFileNameWithoutExtension(path);
                        CreateAndAssignItemId(item, itemName, defaultPath);
                        var so = new SerializedObject(item);
                        so.ApplyModifiedPropertiesWithoutUndo();
                        //
                        
                        database.EditorAdd(item);
                        EditorUtility.SetDirty(database);
                        AssetDatabase.SaveAssets();
                        Selection.activeObject = item;
                    });
            }

            menu.ShowAsContext();
        }

        
        private void CreateItem(ItemCreationTemplate template)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Item",
                template.ToString(),
                "asset",
                "Enter item name",
                settings.basePath);

            if (string.IsNullOrEmpty(path))
                return;

            var item = ScriptableObject.CreateInstance<ItemConfig>();

            AssetDatabase.CreateAsset(item, path);
            
            // ✅ сразу записываем guid от имени конфига
            string itemName = System.IO.Path.GetFileNameWithoutExtension(path);
            CreateAndAssignItemId(item, itemName, path);
            var so = new SerializedObject(item);
            so.ApplyModifiedPropertiesWithoutUndo();
            //
            
            AddModulesByTemplate(item, template);

            EditorUtility.SetDirty(item);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            database.EditorAdd(item);

            Selection.activeObject = item;
        }
        
        private void AddModulesByTemplate(ItemConfig item, ItemCreationTemplate template)
        {
            var so = new SerializedObject(item);
            var modules = so.FindProperty("modules");

            void AddModule(Type type)
            {
                int index = modules.arraySize;
                modules.InsertArrayElementAtIndex(index);

                modules.GetArrayElementAtIndex(index).managedReferenceValue =
                    Activator.CreateInstance(type);
            }

            switch (template)
            {
                case ItemCreationTemplate.Resource:
                    AddModule(typeof(ResourceModule));
                    break;

                case ItemCreationTemplate.Weapon:
                    AddModule(typeof(WeaponModule));
                    break;

                case ItemCreationTemplate.Equipment:
                    AddModule(typeof(EquipmentModule));
                    break;

                case ItemCreationTemplate.VehicleEquipment:
                    AddModule(typeof(VehicleEquipmentModule));
                    break;

                case ItemCreationTemplate.Ammo:
                    AddModule(typeof(AmmoModule));
                    break;

                case ItemCreationTemplate.Storage:
                    AddModule(typeof(StorageModule));
                    break;

                case ItemCreationTemplate.CraftStation:
                    AddModule(typeof(CraftingStationModule));
                    break;

                case ItemCreationTemplate.LivingModule:
                    AddModule(typeof(LivingModule));
                    break;

                case ItemCreationTemplate.Tavern:
                    AddModule(typeof(TavernModule));
                    break;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void CreateAndAssignItemId(ItemConfig item, string itemName, string path)
        {
            // Resolve the category folder for this template type
            string categoryFolder = path;
            string idFolder = categoryFolder + "/_Ids";

            // Then ensure the Ids subfolder exists
            if (!AssetDatabase.IsValidFolder(idFolder))
            {
                AssetDatabase.CreateFolder(categoryFolder, "_Ids");
            }

            string idPath = $"{idFolder}/ID.{itemName}.asset";
            idPath = AssetDatabase.GenerateUniqueAssetPath(idPath);

            var itemId = ScriptableObject.CreateInstance<ItemId>();
            AssetDatabase.CreateAsset(itemId, idPath);

            // === инициализируем guid
            var idSo = new SerializedObject(itemId);
            var guidProp = idSo.FindProperty("guid");
            guidProp.stringValue = Guid.NewGuid().ToString("N");
            idSo.ApplyModifiedProperties();

            EditorUtility.SetDirty(itemId);
            
            // === передаем созданный ItemId в конфиг
            var so = new SerializedObject(item);
            var idProp = so.FindProperty("id");
            idProp.objectReferenceValue = itemId;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(item);
        }


        private string GetPathForType(ItemCategory category)
        {
            if (settings == null)
                return "Assets/_NEW_ITEM";

            var folder = settings.basePath;

            if (category == ItemCategory.Resource)
                return (folder + settings.resourcePath);
            
            if (category == ItemCategory.Consumable)
                return (folder + settings.consumablePath);
            if (category == ItemCategory.Ammo)
                return (folder + settings.ammoPath);
            if (category == ItemCategory.Weapon)
                return (folder + settings.weaponPath);
            if (category == ItemCategory.Armor)
                return (folder + settings.armorPath);
            if (category == ItemCategory.Upgrade)
                return (folder + settings.upgradePath);
            
            if (category == ItemCategory.Vehicle)
                return (folder + settings.vehiclePath);
            
            if (category == ItemCategory.Blueprint)
                return (folder + settings.blueprintPath);
            
            if (category == ItemCategory.Station)
                return (folder + settings.stationPath);
            if (category == ItemCategory.Storage)
                return (folder + settings.storagePath);
            if (category == ItemCategory.DefenseFacility)
                return (folder + settings.defensePath);
            
            // все остальные объекты лагеря
            if (category == ItemCategory.BaseFacility)
                return (folder + settings.campPath);

            return "Assets/_NEW_ITEM";
        }

        

        private void ValidateItems()
        {
            foreach (var item in database.Items)
            {
                if (item == null) continue;
                if (item.Id == null) Debug.LogError($"Item {item.name} has empty GUID");
            }

            EditorUtility.DisplayDialog("Validation", "Validation complete! Check console for warnings.", "OK");
        }

        /// <summary>
        /// Проверяет что предметы лутовых категорий имеют LootModule.
        /// Допустимые категории: Resource, Weapon, Armor, Upgrade, Ammo, Consumable, Blueprint.
        /// Выводит результат в Console и показывает диалог с итогом.
        /// </summary>
        private void ValidateLootModules()
        {
            GConsole.ClearLog();
            
            // Категории которые должны участвовать в луте
            var lootableCategories = new HashSet<ItemCategory>
            {
                ItemCategory.Resource,
                ItemCategory.Weapon,
                ItemCategory.Armor,
                ItemCategory.Upgrade,
                ItemCategory.Ammo,
                ItemCategory.Consumable,
                ItemCategory.Blueprint,
            };

            var missing = new List<ItemConfig>();
            var missingDetails = new System.Text.StringBuilder();

            foreach (var item in database.Items)
            {
                if (item == null) continue;
                if (!lootableCategories.Contains(item.Classification.category)) continue;

                if (!item.HasModule<LootModule>())
                {
                    missing.Add(item);
                    missingDetails.AppendLine(
                        $"  [{item.Classification.category}]  {item.Header.titleLid}  ({item.name})");
                }
            }

            if (missing.Count == 0)
            {
                Debug.Log("[LootModule Validator] ✅ Все предметы лутовых категорий имеют LootModule.");
                EditorUtility.DisplayDialog(
                    "Loot Module Validation",
                    "✅ Все предметы лутовых категорий имеют LootModule.",
                    "OK");
                return;
            }

            // Логируем каждый проблемный предмет со ссылкой — кликабельно в Console
            Debug.Log(
                $"[LootModule Validator] ⚠️ {missing.Count} предмет(ов) без LootModule:\n" +
                missingDetails);

            foreach (var item in missing)
                Debug.Log($"Missing LootModule: {item.Header.titleLid}", item);

            // Диалог с вариантами
            int choice = EditorUtility.DisplayDialogComplex(
                "Loot Module Validation",
                $"⚠️ {missing.Count} предмет(ов) лутовых категорий не имеют LootModule.\n\n" +
                "Добавить LootModule автоматически всем?",
                "Добавить всем", // 0
                "Отмена", // 1
                "Только показать" // 2
            );

            if (choice == 0)
                AddLootModulesToAll(missing);

            // choice == 1 → ничего, choice == 2 → уже залогировано выше
        }
        
        /// <summary>
        /// Автоматически добавляет LootModule всем предметам из списка.
        /// Использует SerializedObject чтобы изменения корректно сохранились.
        /// </summary>
        private void AddLootModulesToAll(List<ItemConfig> items)
        {
            int count = 0;

            foreach (var item in items)
            {
                if (item == null) continue;

                var so      = new SerializedObject(item);
                var modules = so.FindProperty("modules");

                if (modules == null)
                {
                    Debug.LogError($"[LootModule Validator] Не удалось найти поле modules у {item.name}", item);
                    continue;
                }

                // Проверяем ещё раз — вдруг уже добавили в этом же проходе
                if (item.HasModule<LootModule>()) continue;

                int index = modules.arraySize;
                modules.InsertArrayElementAtIndex(index);
                modules.GetArrayElementAtIndex(index).managedReferenceValue = new LootModule();
                so.ApplyModifiedProperties();

                EditorUtility.SetDirty(item);
                count++;

                Debug.Log($"[LootModule Validator] ✅ Добавлен LootModule → {item.Header.titleLid}", item);
            }

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Loot Module Validation",
                $"✅ LootModule добавлен {count} предмет(ам).\n\nНастройте Value, Tier и DropTag в Inspector.",
                "OK");
        }

        private void FixMissingItemIds()
        {
            int count = 0;

            foreach (var item in database.Items)
            {
                if (item == null) continue;
                if (item.Id != null) continue;

                string itemName = item.name;

                // Определяем папку по категории предмета
                string categoryFolder = GetPathForType(item.Classification.category);
        
                CreateAndAssignItemId(item, itemName, categoryFolder);
                EditorUtility.SetDirty(item);
                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Fix Item IDs", $"Created ItemId assets for {count} items.", "OK");
        }
    }
}
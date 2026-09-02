using System.Collections.Generic;
using Galactic1.Configs;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Tools
{
    public class StationFuelEditor
    {
        ItemManagerWindow itemManager;

        private Vector2 rightScroll;
        private Vector2 fuelBoxScroll;

        private ItemDatabase database;
        private List<ItemConfig> fuelItems = new();

        //private ProductionStationConfig _productionStationConfig;


        public StationFuelEditor(ItemManagerWindow itemManager)
        {
            this.itemManager = itemManager;

            itemManager.onNewItemSelected += () =>
            {
            };
            
            LoadDatabase();
        }



        private void LoadDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:ItemDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(path);
            }
        }

        


        public void DrawPanel()
        {
            /*if (database == null)
            {
                EditorGUILayout.HelpBox("ItemDatabase not found! Please create one.", MessageType.Warning);
                if (GUILayout.Button("ReloadSpeed")) LoadDatabase();
                return;
            }

            _productionStationConfig = (ProductionStationConfig)itemManager.selectedItem.Config;
            
            // * собираем все предметы являющиеся топливом
            fuelItems = new List<ItemBase_old>();
            foreach (var item in database.Items)
            {
                if (item.Config && item.Config.FuelSettings.enabled)
                    fuelItems.Add(item);
            }

            EditorGUILayout.BeginHorizontal(); // 🔹 Основная сетка: левая / центр / правая
            {
                // Центральный блок (информация о предмете)
                EditorGUILayout.BeginVertical(GUILayout.Width(400));
                DrawCenterBox();
                DrawDetailsBox();
                EditorGUILayout.EndVertical();

                // Правая колонка (список рецептов)
                // EditorGUILayout.BeginVertical("box", GUILayout.Width(200));
                // DrawRightBox();
                // EditorGUILayout.Space(10);
                // EditorGUILayout.BeginHorizontal();
                // GUILayout.FlexibleSpace();


                // GUILayout.FlexibleSpace();
                // EditorGUILayout.EndHorizontal();
                // EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);*/

        }


        // 🔹 Центральный блок — информация о выбранном предмете
        private void DrawCenterBox()
        {
            if (itemManager.SelectedItem == null)
            {
                EditorGUILayout.HelpBox("Select an item to view details", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            {
                // 🔹 1. Верхний блок — иконка + имя
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();

                if (itemManager.SelectedItem.Header.icon != null)
                    EditorUtils.DrawSprite(itemManager.SelectedItem.Header.icon, 30f);
                else
                {
                    var rect = GUILayoutUtility.GetRect(30, 30,
                        GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                    EditorGUI.DrawRect(rect, Color.black);
                }

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(itemManager.SelectedItem.Header.titleLid, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(itemManager.SelectedItem.name, EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        // 🔹 Новый центральный бокс для редактирования выбранного рецепта
        private void DrawDetailsBox()
        {
            /*EditorGUILayout.BeginVertical("box");
            fuelBoxScroll = EditorGUILayout.BeginScrollView(fuelBoxScroll, GUILayout.Width(400));

            EditorGUILayout.Space(10);

            // 📦 Fuels с кнопкой "+"
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("📦 Available Fuels", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
            {
                IngredientPickerWindow.ShowWindow(fuelItems, (ItemBase_old selectedItem) =>
                {
                    List<ProductionStationConfig.FuelItem> tempList =
                        new(_productionStationConfig.validFuel ?? new ProductionStationConfig.FuelItem[0]);

                    // Проверка дубликата
                    if (!tempList.Exists(x => x.item == selectedItem))
                    {
                        tempList.Add(new ProductionStationConfig.FuelItem() { item = selectedItem });
                        _productionStationConfig.validFuel = tempList.ToArray();
                        EditorUtility.SetDirty(_productionStationConfig);
                    }
                    else
                    {
                        Debug.LogWarning($"Ingredient '{selectedItem.Header.titleLid}' уже добавлен в список!");
                    }
                });
            }

            EditorGUILayout.EndHorizontal();

            if (_productionStationConfig.validFuel == null)
                _productionStationConfig.validFuel = new ProductionStationConfig.FuelItem[0];

            // Рабочая копия списка
            List<ProductionStationConfig.FuelItem> tempList = new(_productionStationConfig.validFuel);

            // Запись Undo один раз перед возможными изменениями (чтобы все изменения были отменяемы)
            // Мы записываем Undo здесь, потому что дальше возможно несколько изменений (удаление, замена, слайдер)
            Undo.RecordObject(_productionStationConfig, "Edit Fuel Items");

            for (int i = 0; i < tempList.Count; i++)
            {
                if (tempList[i].item == null) continue;
                int index = i;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                // Иконка
                if (tempList[index].item.Header.icon != null)
                    EditorUtils.DrawSprite(tempList[index].item.Header.icon, 30f);
                else
                {
                    var rect = GUILayoutUtility.GetRect(30, 30);
                    EditorGUI.DrawRect(rect, Color.gray);
                }

                // Название предмета (кнопка)
                if (GUILayout.Button(tempList[index].item.Header.titleLid, GUILayout.Width(200), GUILayout.Height(30)))
                {
                    IngredientPickerWindow.ShowWindow(fuelItems, (ItemBase_old selectedItem) =>
                    {
                        // Проверка дубликата при изменении
                        if (!tempList.Exists(x => x.item == selectedItem))
                        {
                            tempList[index].item = selectedItem;
                            _productionStationConfig.validFuel = tempList.ToArray();
                            EditorUtility.SetDirty(_productionStationConfig);
                        }
                        else
                        {
                            Debug.LogWarning($"Ingredient '{selectedItem.Header.titleLid}' уже есть в списке!");
                        }
                    });
                }

                // Кнопка удаления
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    tempList.RemoveAt(index);
                    i--;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }

                EditorGUILayout.EndHorizontal();

                // 🔹 Ползунок под блоком
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("🔥 Efficiency Multiplier", EditorStyles.miniBoldLabel);

                // Кешируем старое значение, чтобы определить изменение
                float oldVal = tempList[index].efficiencyMultiplier;
                float newVal = EditorGUILayout.Slider(oldVal, 0.1f, 5f);

                if (!Mathf.Approximately(oldVal, newVal))
                {
                    tempList[index].efficiencyMultiplier = newVal;
                    // сразу сохраняем обратно в конфиг, чтобы изменения были видны и persist
                    _productionStationConfig.validFuel = tempList.ToArray();
                    EditorUtility.SetDirty(_productionStationConfig);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            // В конце — убедимся, что массив обновлён (если ничего не изменилось внутри цикла,
            // но были операции удаления/замены, присвоение всё равно выполнится).
            _productionStationConfig.validFuel = tempList.ToArray();
            EditorUtility.SetDirty(_productionStationConfig);


            EditorGUILayout.Space(10);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();*/
        }




        // 🔹 Правая колонка — список рецептов
        // private void DrawRightBox()
        // {
        //     EditorGUILayout.BeginHorizontal("box");
        //     EditorGUILayout.LabelField("⚙️ Available Fuel", EditorStyles.boldLabel);
        //
        //     ProductionStationConfig crst = null;
        //     if (itemManager.selectedItem != null &&
        //         itemManager.selectedItem.config != null &&
        //         itemManager.selectedItem.config is ProductionStationConfig st)
        //     {
        //         crst = st;
        //     }
        //
        //     GUI.enabled = crst != null;
        //     if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
        //     {
        //         AddNewFuel();
        //     }
        //
        //     GUI.enabled = true;
        //
        //     EditorGUILayout.EndHorizontal();
        //     EditorGUILayout.Space(20);
        //
        //     if (crst == null || crst.validFuel == null || crst.validFuel.Count == 0)
        //     {
        //         EditorGUILayout.HelpBox("No fuel found", MessageType.None);
        //         return;
        //     }
        //
        //     rightScroll = EditorGUILayout.BeginScrollView(rightScroll);
        //
        //     List<int> removeIndices = new List<int>(); // собираем индексы для удаления после цикла
        //
        //     for (int i = 0; i < crst.validFuel.Count; i++)
        //     {
        //         var fuel = crst.validFuel[i].item;
        //         if (fuel == null) continue;
        //
        //         EditorGUILayout.BeginHorizontal();
        //
        //         if (fuel == selectedFuel)
        //             GUI.backgroundColor = Color.cyan;
        //
        //         if (GUILayout.Button($"Fuel {i + 1}", GUILayout.Height(25)))
        //         {
        //             selectedFuel = fuel;
        //             Selection.activeObject = fuel;
        //             EditorGUIUtility.PingObject(fuel);
        //         }
        //
        //         if (fuel == selectedFuel)
        //             GUI.backgroundColor = itemManager.original;
        //
        //         // Кнопка удаления — помечаем для подтверждения
        //         if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
        //         {
        //             fuelToDelete.Add(i);
        //         }
        //
        //         EditorGUILayout.EndHorizontal();
        //
        //         // Если рецепт помечен для удаления — показываем подтверждение
        //         if (fuelToDelete.Contains(i))
        //         {
        //             EditorGUILayout.BeginVertical("box");
        //             EditorGUILayout.HelpBox($"Are you sure you want to delete Fuel {i + 1}?", MessageType.Warning);
        //
        //             EditorGUILayout.BeginHorizontal();
        //             if (GUILayout.Button("Yes", GUILayout.Width(50)))
        //             {
        //                 removeIndices.Add(i); // удаляем после цикла
        //                 fuelToDelete.Remove(i);
        //             }
        //
        //             if (GUILayout.Button("No", GUILayout.Width(50)))
        //             {
        //                 fuelToDelete.Remove(i); // отмена удаления
        //             }
        //
        //             EditorGUILayout.EndHorizontal();
        //             EditorGUILayout.EndVertical();
        //         }
        //     }
        //
        //     EditorGUILayout.EndScrollView();
        //
        //     // Удаление после цикла, чтобы избежать ошибки GUILayout
        //     for (int j = removeIndices.Count - 1; j >= 0; j--)
        //     {
        //         int index = removeIndices[j];
        //         var fuel = crst.validFuel[index];
        //
        //         if (fuel != null)
        //         {
        //             // Удаляем ассет
        //             string path = AssetDatabase.GetAssetPath(fuel);
        //             if (!string.IsNullOrEmpty(path))
        //                 AssetDatabase.DeleteAsset(path);
        //
        //             // Удаляем из базы
        //             if (recipeDatabase != null && recipeDatabase.recipes.Contains(fuel))
        //             {
        //                 recipeDatabase.recipes.Remove(fuel);
        //                 EditorUtility.SetDirty(recipeDatabase);
        //             }
        //         }
        //
        //         // Удаляем из предмета
        //         itemManager.selectedItem.config.recipes.RemoveAt(index);
        //         EditorUtility.SetDirty(itemManager.selectedItem.config);
        //     }
        // }


        // 🔹 Создание нового RecipeConfig
        // private void AddNewFuel()
        // {
        //     if (itemManager.selectedItem == null || itemManager.selectedItem.config == null)
        //         return;
        //
        //     var config = itemManager.selectedItem.config;
        //
        //     // Получаем путь к конфигу предмета
        //     string configPath = AssetDatabase.GetAssetPath(config);
        //     if (string.IsNullOrEmpty(configPath))
        //     {
        //         Debug.LogWarning("Can't find asset path for item config.");
        //         return;
        //     }
        //
        //     // Папка предмета
        //     string folderPath = Path.GetDirectoryName(configPath);
        //     if (!AssetDatabase.IsValidFolder(folderPath))
        //         return;
        //
        //     // Создаём новый рецепт
        //     string recipeName = $"{itemManager.selectedItem.name}_Recipe_{config.recipes.Count + 1}.asset";
        //     string recipePath = Path.Combine(folderPath, recipeName).Replace("\\", "/");
        //
        //     var newRecipe = ScriptableObject.CreateInstance<RecipeConfig>();
        //     newRecipe.name = Path.GetFileNameWithoutExtension(recipeName);
        //     newRecipe.outputItem = itemManager.selectedItem;
        //     newRecipe.outputAmount = 1;
        //
        //     AssetDatabase.CreateAsset(newRecipe, recipePath);
        //     AssetDatabase.SaveAssets();
        //     AssetDatabase.Refresh();
        //
        //     // Добавляем в список рецептов предмета
        //     config.recipes.Add(newRecipe);
        //     EditorUtility.SetDirty(config);
        //
        //     // 🔹 Добавляем в центральную базу рецептов
        //     if (recipeDatabase != null && !recipeDatabase.recipes.Contains(newRecipe))
        //     {
        //         recipeDatabase.recipes.Add(newRecipe);
        //         EditorUtility.SetDirty(recipeDatabase);
        //         AssetDatabase.SaveAssets();
        //         AssetDatabase.Refresh();
        //     }
        //
        //     Debug.Log($"Created new recipe for {itemManager.selectedItem.name} → {recipePath}");
        // }



    }
}
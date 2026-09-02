using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using Galactic1.Code.Systems.Economy;
using Galactic1.Core.Enums;
using Galactic1.Crafting;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;

namespace Galactic1.Tools
{
    public class RecipesEditor
    {
        private ItemManagerWindow itemManager;
        private RecipeDatabase recipeDatabase;

        private Vector2 rightScroll;
        private Vector2 recipeBoxScroll;

        private ItemDatabase database;
        public ItemCategoryEditor _selectedCategoryEditor = ItemCategoryEditor.All;
        private CraftRecipeConfig selectedRecipe; // текущий выбранный рецепт
        private int recipeToDelete = -1; // индекс рецепта для подтверждения удаления
        private HashSet<int> recipesToDelete = new HashSet<int>();




        public RecipesEditor(ItemManagerWindow itemManager)
        {
            this.itemManager = itemManager;

            itemManager.onNewItemSelected += () =>
            {
                selectedRecipe = null;
            };
            
            LoadDatabase();
            LoadRecipeDatabase();
        }
        
        void PropertyField(SerializedProperty property, string label)
        {
            EditorGUILayout.PropertyField(property, new GUIContent(label));
        }
        void PropertyField(SerializedProperty property, string target, string label)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative(target), new GUIContent(label));
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

        private void LoadRecipeDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:RecipeDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                recipeDatabase = AssetDatabase.LoadAssetAtPath<RecipeDatabase>(path);
            }
            else
            {
                // Создать новую базу, если не существует
                recipeDatabase = ScriptableObject.CreateInstance<RecipeDatabase>();
                AssetDatabase.CreateAsset(recipeDatabase, "Assets/RecipeDatabase.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Created new RecipeDatabase");
            }
        }
        


        public void DrawPanel()
        {
            if (database == null)
            {
                EditorGUILayout.HelpBox("ItemDatabase not found! Please create one.", MessageType.Warning);
                if (GUILayout.Button("ReloadSpeed")) LoadDatabase();
                return;
            }


            EditorGUILayout.BeginHorizontal(); // 🔹 Основная сетка: левая / центр / правая
            {
                // Центральный блок (информация о предмете)
                EditorGUILayout.BeginVertical(GUILayout.Width(400));
                DrawCenterBox();
                DrawRecipeDetailsBox();
                EditorGUILayout.EndVertical();

                // Правая колонка (список рецептов)
                EditorGUILayout.BeginVertical("box", GUILayout.Width(200));
                DrawRightBox();
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("🧩 Validate Recipes", GUILayout.Width(180), GUILayout.Height(25)))
                {
                    ValidateAllItems();
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

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
                EditorGUILayout.SelectableLabel(itemManager.SelectedItem.name, 
                    EditorStyles.miniLabel, GUILayout.Height(18));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        // 🔹 Новый центральный бокс для редактирования выбранного рецепта
        private void DrawRecipeDetailsBox()
        {
            if (selectedRecipe == null)
            {
                EditorGUILayout.HelpBox("Select a recipe to edit details", MessageType.Info);
                return;
            }
            
            Undo.RecordObject(selectedRecipe, "Modify Recipe");
            
            SerializedObject so = new SerializedObject(selectedRecipe);

            SerializedProperty outputItemProp = so.FindProperty("outputItem");
            SerializedProperty outputCountProp = so.FindProperty("outputCount");
            SerializedProperty stationProp = so.FindProperty("requiredStation");
            SerializedProperty craftTimeProp = so.FindProperty("craftTime");
            SerializedProperty stackOrderProp = so.FindProperty("stackOrderLimit");
            SerializedProperty requiresProp = so.FindProperty("requires");

            so.Update();

            EditorGUI.BeginChangeCheck();
            
            

            EditorGUILayout.BeginVertical("box");
            recipeBoxScroll = EditorGUILayout.BeginScrollView(recipeBoxScroll, GUILayout.Width(400));

            EditorGUILayout.LabelField("🛠 Recipe Details", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // outputAmount
            PropertyField(craftTimeProp, "Craft Time");
            PropertyField(outputCountProp, "Output Count");
            PropertyField(stackOrderProp, "Stack Order");

            // requiredStation
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Required Station", GUILayout.Width(150));

            if (selectedRecipe.RequiredStationItem != null)
            {
                EditorUtils.DrawSprite(selectedRecipe.RequiredStationItem.Header.icon, 20);
                GUILayout.Label(selectedRecipe.RequiredStationItem.Header.titleLid);
            }
            else
            {
                GUILayout.Label("...");
            }

            if (GUILayout.Button("Select", GUILayout.Width(50)))
            {
                var allStations = database.GetAllCraftStation();
                var craftStationModules = allStations
                    .Select(item => item.CraftStation)                // выбираем сам модуль
                    .ToList();                                        // превращаем в List<CraftingStationModule>
                

                StationPickerWindow.ShowWindow(craftStationModules, (CraftingStationModule selectedStation) =>
                {
                    selectedRecipe.SetRequiredStation(selectedStation.Item);
                });
            }

            EditorGUILayout.EndHorizontal();

            // craftTime
            EditorGUILayout.Space(3);
            PropertyField(so.FindProperty("requiredTier"), "Required Tier");

            EditorGUILayout.Space(10);

            // 📦 Ingredients с кнопкой "+"
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("📦 Ingredients", EditorStyles.boldLabel);
            
            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
            {
                IngredientPickerWindow.ShowWindow(
                    _selectedCategoryEditor, 
                    database.Items, 
                    (ItemConfig selectedItem) =>
                {
                    if (selectedRecipe == null)
                        return;

                    selectedRecipe.AddIngredient(selectedItem);

                    EditorUtility.SetDirty(selectedRecipe);
                    AssetDatabase.SaveAssets();

                });
            }

            EditorGUILayout.EndHorizontal();

            
             List<RequirementData> tempList = new List<RequirementData>(selectedRecipe.Requirement);

             for (int i = 0; i < tempList.Count; i++)
             {
                 var req = tempList[i];
                 
                 // * подсвечиваем пустой элемент
                 if (req == null || req.Item == null)
                 {
                     GUI.backgroundColor = Color.red;
                     EditorGUILayout.BeginHorizontal("box");
                     GUILayout.Label($"Пустой элемент '{i}'");
                     EditorGUILayout.EndHorizontal();
                     GUI.backgroundColor = Color.white;
                     continue;
                 }
                 
            
                 int index = i;
                 EditorGUILayout.BeginHorizontal("box");
            
                 // Иконка
                 if (req.Item.Header.icon != null)
                     EditorUtils.DrawSprite(req.Item.Header.icon, 30f);
            
                 // Название предмета
                 if (GUILayout.Button(req.Item.Header.titleLid, GUILayout.Width(200), GUILayout.Height(30)))
                 {
                     IngredientPickerWindow.ShowWindow(
                         _selectedCategoryEditor, 
                         database.Items, 
                         (ItemConfig selectedItem) =>
                     {
                         // Проверка дубликата при изменении
                         if (!tempList.Exists(x => x.Item == selectedItem))
                         {
                             tempList[index].Item = selectedItem;
                             selectedRecipe.SetIngredients(tempList);
                             EditorUtility.SetDirty(selectedRecipe);
                         }
                         else
                         {
                             Debug.LogWarning($"Ingredient '{selectedItem.Header.titleLid}' уже есть в рецепте!");
                         }
                     });
                 }
            
                 // amount
                 int newAmount = EditorGUILayout.IntField(req.Amount, GUILayout.Width(50));
                 if (newAmount != req.Amount)
                 {
                     tempList[i].Amount = newAmount;

                     selectedRecipe.SetIngredients(tempList);

                     EditorUtility.SetDirty(selectedRecipe);
                 }
            
                 // delete
                 GUI.backgroundColor = Color.red;
                 if (GUILayout.Button("x", GUILayout.Width(20)))
                 {
                     tempList.RemoveAt(index);
                     selectedRecipe.SetIngredients(tempList);
                     i--;
                 }
                 GUI.backgroundColor = Color.white;
            
                 EditorGUILayout.EndHorizontal();
             }



            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(selectedRecipe);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }







        // 🔹 Правая колонка — список рецептов
        private void DrawRightBox()
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("⚙️ Recipes", EditorStyles.boldLabel);

            GUI.enabled = itemManager.SelectedItem != null &&
                          itemManager.SelectedItem.IsCraftable;
            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
            {
                CreateNewRecipe();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            if (itemManager.SelectedItem == null ||  
                itemManager.SelectedItem.Recipes == null)
            {
                EditorGUILayout.HelpBox("No recipes found", MessageType.None);
                return;
            }

            rightScroll = EditorGUILayout.BeginScrollView(rightScroll);

            List<int> removeIndices = new List<int>(); // собираем индексы для удаления после цикла

            for (int i = 0; i < itemManager.SelectedItem.Recipes.Count; i++)
            {
                var recipe = itemManager.SelectedItem.Recipes[i];
                if (recipe == null) continue;

                EditorGUILayout.BeginHorizontal();

                if (recipe == selectedRecipe)
                    GUI.backgroundColor = Color.cyan;

                if (GUILayout.Button($"Recipe {i + 1}", GUILayout.Height(25)))
                {
                    selectedRecipe = recipe;
                    Selection.activeObject = recipe;
                    EditorGUIUtility.PingObject(recipe);
                }

                if (recipe == selectedRecipe)
                    GUI.backgroundColor = itemManager.original;

                // Кнопка удаления — помечаем для подтверждения
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    recipesToDelete.Add(i);
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();

                // Если рецепт помечен для удаления — показываем подтверждение
                if (recipesToDelete.Contains(i))
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.HelpBox($"Are you sure you want to delete Recipe {i + 1}?", MessageType.Warning);

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Yes", GUILayout.Width(50)))
                    {
                        removeIndices.Add(i); // удаляем после цикла
                        recipesToDelete.Remove(i);
                    }

                    if (GUILayout.Button("No", GUILayout.Width(50)))
                    {
                        recipesToDelete.Remove(i); // отмена удаления
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndScrollView();

            // Удаление после цикла, чтобы избежать ошибки GUILayout
            for (int j = removeIndices.Count - 1; j >= 0; j--)
            {
                int index = removeIndices[j];
                var recipe = itemManager.SelectedItem.Recipes[index];

                if (recipe != null)
                {
                    // Удаляем ассет
                    string path = AssetDatabase.GetAssetPath(recipe);
                    if (!string.IsNullOrEmpty(path))
                        AssetDatabase.DeleteAsset(path);

                    // Удаляем из базы
                    if (recipeDatabase != null && recipeDatabase.recipes.Contains(recipe))
                    {
                        recipeDatabase.recipes.Remove(recipe);
                        EditorUtility.SetDirty(recipeDatabase);
                    }
                }

                // Удаляем из предмета
                itemManager.SelectedItem.RemoveRecipe(index);
                    
                UpdateAfterDelete();
                EditorUtility.SetDirty(itemManager.SelectedItem);
            }
        }


        // 🔹 Создание нового RecipeConfig
        private void CreateNewRecipe()
        {
            if (itemManager.SelectedItem == null)
                return;

            var config = itemManager.SelectedItem;

            // Получаем путь к конфигу предмета
            string configPath = AssetDatabase.GetAssetPath(config);
            if (string.IsNullOrEmpty(configPath))
            {
                Debug.LogWarning("Can't find asset path for item config.");
                return;
            }

            // Папка предмета
            string folderPath = Path.GetDirectoryName(configPath);
            if (!AssetDatabase.IsValidFolder(folderPath))
                return;

            // Создаём новый рецепт
            string recipeName = $"{itemManager.SelectedItem.name}_Recipe_{config.Recipes.Count + 1}.asset";
            string recipePath = Path.Combine(folderPath, recipeName).Replace("\\", "/");

            var newRecipe = ScriptableObject.CreateInstance<CraftRecipeConfig>();
            newRecipe.name = Path.GetFileNameWithoutExtension(recipeName);
            //newRecipe.outputAmount = itemManager.selectedItem;
            //newRecipe.outputAmount = 1;
            
            SerializedObject so = new SerializedObject(newRecipe);

            so.FindProperty("outputItem").objectReferenceValue = itemManager.SelectedItem;
            so.FindProperty("outputCount").intValue = 1;

            
            

            AssetDatabase.CreateAsset(newRecipe, recipePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Добавляем в список рецептов предмета
            config.AddRecipe(newRecipe);
            EditorUtility.SetDirty(config);

            // 🔹 Добавляем в центральную базу рецептов
            if (recipeDatabase != null && !recipeDatabase.recipes.Contains(newRecipe))
            {
                recipeDatabase.recipes.Add(newRecipe);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(recipeDatabase);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"Created new recipe for {itemManager.SelectedItem.name} → {recipePath}");
        }

        void UpdateAfterDelete()
        {
            var config = itemManager.SelectedItem;
            string itemName = itemManager.SelectedItem.name;

            // Переименование существующих рецептов
            for (int i = 0; i < config.Recipes.Count; i++)
            {
                var recipe = config.Recipes[i];
                if (recipe == null)
                    continue;

                string newName = $"{itemName}_Recipe_{i + 1}";
                string assetPath = AssetDatabase.GetAssetPath(recipe);

                if (!string.IsNullOrEmpty(assetPath))
                {
                    AssetDatabase.RenameAsset(assetPath, newName);
                }

                recipe.name = newName;
                EditorUtility.SetDirty(recipe);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }




        private void ValidateAllItems()
        {
            if (database == null || database.Items == null)
            {
                EditorUtility.DisplayDialog("Validation", "ItemDatabase not found or empty.", "OK");
                return;
            }

            List<string> issues = new List<string>();
            int totalRecipes = 0;

            foreach (var item in database.Items)
            {
                if (item == null || item.Recipes == null) 
                    continue;

                var recipes = item.Recipes;
                totalRecipes += recipes.Count;

                // 1️⃣ Проверка пустых ссылок
                for (int i = 0; i < recipes.Count; i++)
                {
                    var recipe = recipes[i];
                    if (recipe == null)
                    {
                        issues.Add($"'{item.Header.titleLid}' \nRecipe #{i + 1} is null.");
                        continue;
                    }

                    if (recipe.RequiredStationItem == null)
                        issues.Add($"'{item.Header.titleLid}' \nRecipe #{i + 1} missing required station.");

                    if (recipe.Requirement == null || recipe.Requirement.Count == 0)
                        issues.Add($"'{item.Header.titleLid}' \nRecipe #{i + 1} has no ingredients.");
                    else
                    {
                        for (int j = 0; j < recipe.Requirement.Count; j++)
                        {
                            if (recipe.Requirement[j].Item == null)
                                issues.Add($"'{item.Header.titleLid}' \nRecipe #{i + 1} ingredient #{j + 1} is null.");
                            if (recipe.Requirement[j].Amount == 0)
                                issues.Add($"'{item.Header.titleLid}' \nRecipe #{i + 1} ingredient #{j + 1} x0");
                        }
                    }
                }

                // 2️⃣ Проверка дубликатов по ингредиентам
                for (int i = 0; i < recipes.Count; i++)
                {
                    var a = recipes[i];
                    if (a == null || a.Requirement == null) continue;

                    for (int j = i + 1; j < recipes.Count; j++)
                    {
                        var b = recipes[j];
                        if (b == null || b.Requirement == null) continue;

                        if (AreRecipesEqual(a, b))
                            issues.Add(
                                $"'{item.Header.titleLid}' \nRecipe #{i + 1} and Recipe #{j + 1} have identical ingredients!");
                    }
                }
            }

            // 3️⃣ Вывод результата
            if (issues.Count == 0)
                EditorUtility.DisplayDialog("Validation Complete", $"✅ All {totalRecipes} recipes are valid!", "OK");
            else
                EditorUtility.DisplayDialog("Validation Results", string.Join("\n", issues), "OK");

            Debug.Log(
                $"[Recipe Validator] Checked {totalRecipes} recipes across {database.Items.Count} items. Found {issues.Count} issues.");
        }


        private bool AreRecipesEqual(CraftRecipeConfig a, CraftRecipeConfig b)
        {
            if (a.Requirement.Count != b.Requirement.Count)
                return false;

            // Сравниваем по составу ингредиентов
            for (int i = 0; i < a.Requirement.Count; i++)
            {
                var reqA = a.Requirement[i];
                bool match = false;

                foreach (var reqB in b.Requirement)
                {
                    if (reqA.Item == reqB.Item && reqA.Amount == reqB.Amount)
                    {
                        match = true;
                        break;
                    }
                }

                if (!match)
                    return false;
            }

            return true;
        }


        
        /// <summary>
        /// true - конфиг имеет проблемы в рецептах
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public bool RecipeError(CraftRecipeConfig recipe)
        {
            var tempList = new List<RequirementData>(recipe.Requirement);

            if (tempList.Count == 0)
                return true;

            for (int i = 0; i < tempList.Count; i++)
            {
                var req = tempList[i];

                if (req == null || req.Item == null)
                    return true;
            }

            return false;
        }
    }
}
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;
using Object = UnityEngine.Object;

namespace Galactic1.Tools
{
    public class ItemManagerWindow : ExtendedEditorWindow
    {


        public ItemDatabase database { get; private set; }
        public ItemManagerSettings settings { get; private set; }

        public static ItemManagerWindow Manager { get; private set; }

        private ItemManagerWindow_BottomToolbar bottomToolbar;

        private StationFuelEditor stationFuelEditor;
        private RecipesEditor recipesEditor;
        private BalanceEditor balanceEditor;
        private ItemActionsEditor _itemActionsEditor;
        private FacilityUpgradesEditor _facilityUpgradesEditor;

        private int mainTab;

        string[] tMainTabs = new string[]
        {
            "Items",
            "Recipes",
            "Balance",
            "Params",
            "Upgrades",
            "Fuel",
        };

        public ItemCategoryEditor selectedCategoryEditor = ItemCategoryEditor.All;
        private Vector2 leftScroll;
        private Vector2 centerScroll;
        public ItemConfig SelectedItem { get; private set; }
        public SerializedObject CurrentSO { get; private set; }
        public RecipesEditor RecipesEditor => recipesEditor;



        public Action onNewItemSelected;
        private bool breakdownPerUnit;


        #region Breakdown Recipe

        private const int BreakdownMaxDepth = 8;
        private const float BreakdownRowHeight = 28f;
        private const float BreakdownIndent = 20f;

        private static readonly Color BreakdownRootBg = new Color(0.25f, 0.55f, 0.95f, 0.20f);
        private static readonly Color BreakdownRootAccent = new Color(0.25f, 0.55f, 0.95f, 0.95f);
        private static readonly Color BreakdownChildBg = new Color(0f, 0f, 0f, 0.15f);
        private static readonly Color BreakdownLineColor = new Color(1f, 1f, 1f, 0.35f);

        private string pendingTooltip;
        private GUIStyle _bdName, _bdMini, _bdMiniItalic, _bdError;


        #endregion


        private static readonly Color CraftableAccentColor = new Color(0.11f, 0.62f, 0.46f, 0.85f); // teal
        private static readonly Color CraftableBgColor = new Color(0.11f, 0.62f, 0.46f, 0.08f);

        private static readonly Color ErrorAccentColor = new Color(0.85f, 0.16f, 0.16f, 0.9f); // red





        [MenuItem("Tools/Items/Item Manager")]
        public static void ShowWindow() => GetWindow<ItemManagerWindow>("Item Manager");

        private void OnEnable()
        {
            Manager = this;
            stationFuelEditor = new StationFuelEditor(this);
            recipesEditor = new RecipesEditor(this);
            balanceEditor = new BalanceEditor(this);
            _itemActionsEditor = new ItemActionsEditor(this);
            _facilityUpgradesEditor = new FacilityUpgradesEditor(this);
            wantsMouseMove = true;

            // * для сброса с вкладки топлива
            onNewItemSelected += () =>
            {
                AssetDatabase.SaveAssets();
                if (mainTab == 5 &&
                    (selectedCategoryEditor != ItemCategoryEditor.Station ||
                     selectedCategoryEditor != ItemCategoryEditor.All ||
                     !SelectedItem.HasModule<CraftingStationModule>()) ||
                    // что бы апгрейд отображался только для зданий
                    mainTab == 4 && !SelectedItem.IsFacility())
                {
                    mainTab = 0;
                }
            };


            LoadDatabase();
            LoadSettings();
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

        private void LoadSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:ItemManagerSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = AssetDatabase.LoadAssetAtPath<ItemManagerSettings>(path);
            }
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.MouseMove)
                Repaint();

            // Сброс фокуса при любом клике левой кнопкой мыши
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                // Проверяем, что клик не по полю ввода (не обязательно, но полезно)
                GUI.FocusControl(null);
                Repaint(); // Обновляем окно, чтобы изменения сразу отразились
            }


            if (bottomToolbar == null)
            {
                bottomToolbar = new ItemManagerWindow_BottomToolbar(this);
            }

            if (database == null)
            {
                EditorGUILayout.HelpBox("ItemDatabase not found! Please create one.", MessageType.Warning);
                if (GUILayout.Button("Create ItemDatabase")) CreateDatabase();
                return;
            }

            EditorGUILayout.BeginHorizontal();

            DrawLeftColumn();
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            TopToolBar();

            switch (mainTab)
            {
                case 0:
                    DrawCenterPanel();
                    break;
                case 1:
                    recipesEditor.DrawPanel();
                    break;
                case 2:
                    balanceEditor.DrawPanel();
                    break;
                case 3:
                    _itemActionsEditor.DrawPanel();
                    break;
                case 4:
                    _facilityUpgradesEditor.DrawPanel();
                    break;
                case 5:
                    //stationFuelEditor.DrawPanel();
                    break;


            }

            EditorGUILayout.EndVertical();


            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            bottomToolbar.DrawBottomToolbar();
            EditorGUILayout.Space(10);
        }

        private void DrawLeftColumn()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200));

            // Бокс с EnumPopup
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📂 Category", EditorStyles.boldLabel);
            var newCategory = (ItemCategoryEditor)EditorGUILayout.EnumPopup(selectedCategoryEditor);
            SelectCategory(newCategory, () =>
            {
                selectedCategoryEditor = newCategory;
                SelectedItem = null;
                CurrentSO = null;
                onNewItemSelected?.Invoke();
            });
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Бокс со списком предметов
            EditorGUILayout.BeginVertical("box");
            leftScroll = EditorGUILayout.BeginScrollView(leftScroll);


            foreach (var item in FilteredItems())
            {
                EditorGUILayout.BeginHorizontal();

                // Мини-иконка 30x30
                if (item.Header.icon != null)
                    EditorUtils.DrawSprite(item.Header.icon, 30f);
                else
                {
                    var rect = GUILayoutUtility.GetRect(30, 30,
                        GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                    EditorGUI.DrawRect(rect, Color.black);
                }

                if (item == SelectedItem)
                    GUI.backgroundColor = Color.green;

                // Кнопка с именем предмета, одинаковой длины и высотой 20
                if (GUILayout.Button(item.Header.titleLid,
                        GUILayout.Width(155), GUILayout.Height(30)))
                {
                    if (SelectedItem != item)
                    {
                        SelectedItem = item;
                        CurrentSO = new SerializedObject(item);
                        onNewItemSelected?.Invoke();
                    }
                }

                // Полоса поверх уже отрисованной кнопки
                if (item.IsCraftable && Event.current.type == EventType.Repaint)
                {
                    Rect last = GUILayoutUtility.GetLastRect();
                    EditorGUI.DrawRect(new Rect(last.x, last.y, last.width, last.height), CraftableBgColor);
                    EditorGUI.DrawRect(new Rect(last.x, last.y, 2.5f, last.height), CraftableAccentColor);
                }

                // алерт об ошибке рецепта
                if (item.IsCraftable &&
                    (item.Recipes == null || item.Recipes.Count == 0 || recipesEditor.RecipeError(item.Recipes[0])))
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        Rect last = GUILayoutUtility.GetLastRect();
                        const float stripeWidth = 2.5f;
                        EditorGUI.DrawRect(
                            new Rect(last.xMax - stripeWidth, last.y, stripeWidth, last.height),
                            ErrorAccentColor);
                    }
                }

                GUI.backgroundColor = original;

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical(); // конец бокса списка предметов

            EditorGUILayout.EndVertical(); // конец всей колонки
        }


        void TopToolBar()
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(30));

            //var _tab = GUILayout.Toolbar(mainTab, tMainTabs, GUILayout.Width(300) ,GUILayout.Height(25));

            var _tab = mainTab;

            DrawTabButton(0, ref _tab);
            DrawTabButton(1, ref _tab);
            DrawTabButton(2, ref _tab);
            DrawTabButton(3, ref _tab);

            // === upgrage level
            GUI.enabled = SelectedItem != null ? SelectedItem.IsFacility() : false;
            DrawTabButton(4, ref _tab);
            GUI.enabled = true;

            // === fuel
            GUI.enabled = FuelButtonActive();
            DrawTabButton(5, ref _tab);
            GUI.enabled = true;


            // выбор вкладки
            if (_tab != mainTab)
            {
                mainTab = _tab;
                balanceEditor.RefreshList();
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawTabButton(int target, ref int tab)
        {
            if (tab == target) GUI.backgroundColor = Color.green;
            if (GUILayout.Button(tMainTabs[target], GUILayout.Width(75), GUILayout.Height(25)))
            {
                tab = target;
            }

            GUI.backgroundColor = original;
        }


        private void DrawCenterPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(500)); // фиксированная ширина центрального бокса

            if (SelectedItem != null)
            {
                centerScroll = EditorGUILayout.BeginScrollView(centerScroll, GUILayout.Width(500));

                // 1. Верхний бокс: имя, редкость, уровень + большая иконка
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                // Левая часть: имя, редкость, уровень
                EditorGUILayout.BeginVertical(GUILayout.Width(350));
                EditorGUILayout.SelectableLabel(SelectedItem.Id?.DebugKey ?? "ItemKey: ----",
                    EditorStyles.miniLabel, GUILayout.Height(18));
                //EditorGUILayout.LabelField(!string.IsNullOrEmpty(SelectedItem.ConfigId) ? SelectedItem.ConfigId : "ItemKey: ----");
                //EditorGUI.BeginDisabledGroup(true);
                //EditorGUILayout.TextField(!string.IsNullOrEmpty(selectedItem.ItemKey) ? selectedItem.Guid : "GUID: ----");
                //EditorGUI.EndDisabledGroup();

                EditorGUILayout.LabelField(!string.IsNullOrEmpty(SelectedItem.Header.titleLid)
                    ? SelectedItem.Header.titleLid
                    : "Title: ----", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(!string.IsNullOrEmpty(SelectedItem.Header.descriptionLid)
                    ? SelectedItem.Header.descriptionLid
                    : "Description: ----", EditorStyles.boldLabel);


                EditorGUILayout.LabelField($"Rarity: {SelectedItem.Classification.rarity}");
                //EditorGUILayout.LabelField($"Level: {stats.Level}");

                EditorGUILayout.EndVertical();

                // Правая часть: большая иконка 80x80
                float iconSize = 80f;
                Rect iconRect = GUILayoutUtility.GetRect(iconSize, iconSize, GUILayout.Width(iconSize),
                    GUILayout.Height(iconSize));

                if (SelectedItem.Header.icon != null)
                {
                    Sprite spr = SelectedItem.Header.icon;
                    Texture2D tex = spr.texture;
                    Rect texCoords = new Rect(
                        spr.rect.x / tex.width,
                        spr.rect.y / tex.height,
                        spr.rect.width / tex.width,
                        spr.rect.height / tex.height
                    );

                    GUI.DrawTextureWithTexCoords(iconRect, tex, texCoords);
                }
                else
                {
                    EditorGUI.DrawRect(iconRect, Color.black);
                    GUI.Label(iconRect, "No Icon", EditorStyles.centeredGreyMiniLabel);
                }

                // 🟢 Добавляем реакцию на клик мышью по иконке
                if (Event.current.type == EventType.MouseDown &&
                    Event.current.button == 0 &&
                    iconRect.Contains(Event.current.mousePosition))
                {
                    EditorGUIUtility.PingObject(SelectedItem); // 🔔 Пингуем сам ItemConfig
                    Event.current.Use(); // предотвращаем дальнейшую обработку клика
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical(); // конец верхнего бокса

                // отображаем настройки для лута
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginVertical("box");
                DrawLootData(SelectedItem);
                EditorGUILayout.EndVertical();

                // 2. Бокс со статистикой
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);



                // Weapon Definition
                if (SelectedItem.HasModule<WeaponModule>())
                {
                    var weapon = SelectedItem.Weapon;
                    var def = weapon.Definition;

                    if (def == null)
                    {
                        EditorGUILayout.Space(5);
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField("⚔ Weapon Definition", EditorStyles.boldLabel);

                        EditorGUILayout.HelpBox("Weapon Definition не назначен", MessageType.Warning);
                        EditorGUILayout.Space(3);
                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        EditorGUILayout.Space(5);
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField("⚔ Weapon Definition", EditorStyles.boldLabel);
                        EditorGUILayout.Space(3);

                        // Fire
                        DrawStatRow2("Fire Mode", def.fireMode.ToString());
                        DrawStatRow2("Fire Type", def.fireType.ToString());
                        DrawStatRow2("Ammo Type", def.ammoType.ToString());

                        EditorGUILayout.Space(5);

                        // Damage
                        EditorGUILayout.LabelField("Damage", EditorStyles.miniBoldLabel);
                        DrawStatRow2("Damage", $"{def.damage} ±{def.damageVariance * 100f:0}%");
                        DrawStatRow2("DPS", $"{weapon.DPS:0.0}");
                        DrawStatRow2("Armor Pierce", $"{def.armorPiercing}");
                        //DrawStatRow2("Range",         $"{def.range}m  (eff: {def.effectiveRange}m / max: {def.maxRange}m)");

                        EditorGUILayout.Space(5);

                        // Fire Rate
                        EditorGUILayout.LabelField("Fire Rate", EditorStyles.miniBoldLabel);
                        DrawStatRow2("RPM", $"{def.roundsPerMinute}");
                        if (def.fireMode == FireMode.Burst)
                        {
                            DrawStatRow2("Burst Count", $"{def.burstCount}");
                            DrawStatRow2("Burst Pause", $"{def.burstPauseSec}s");
                        }

                        if (def.projectilesPerShot > 1)
                            DrawStatRow2("Pellets", $"{def.projectilesPerShot}");

                        EditorGUILayout.Space(5);

                        // Ammo
                        EditorGUILayout.LabelField("Ammo", EditorStyles.miniBoldLabel);
                        DrawStatRow2("Magazine", $"{def.magazineSize}");
                        DrawStatRow2("Reload", $"{def.reloadTimeSec}s");

                        EditorGUILayout.Space(5);

                        // Spread
                        EditorGUILayout.LabelField("Spread", EditorStyles.miniBoldLabel);
                        DrawStatRow2("Accuracy", $"{(int)def.GetAccuracyScore()}%");
                        DrawStatRow2("Base Spread", $"{def.baseSpreadDeg}°");
                        DrawStatRow2("Moving Mul", $"x{def.movingSpreadMul}");
                        DrawStatRow2("Stress Mul", $"x{def.stressSpreadMul}");
                        //DrawStatRow2("Range Penalty", $"x{def.maxRangeSpreadPenalty}");

                        if (def.hasHeat)
                        {
                            EditorGUILayout.Space(5);
                            EditorGUILayout.LabelField("Heat", EditorStyles.miniBoldLabel);
                            DrawStatRow2("Per Shot", $"{def.heatPerShot}");
                            DrawStatRow2("Cool Rate", $"{def.heatCoolRate}/s");
                            DrawStatRow2("Overheat", $"{def.overheatThreshold}");
                            DrawStatRow2("Cooldown", $"{def.cooldownSec}s");
                        }

                        if (def.hasSuppression)
                        {
                            EditorGUILayout.Space(5);
                            EditorGUILayout.LabelField("Suppression", EditorStyles.miniBoldLabel);
                            DrawStatRow2("Angle", $"{def.suppressionAngle}°");
                            DrawStatRow2("Range", $"{def.suppressionRange}m");
                        }

                        EditorGUILayout.EndVertical();

                        // Ammo Definition
                        var ammoDef = def.supportedAmmo;
                        if (ammoDef != null)
                        {
                            EditorGUILayout.Space(5);
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField("🔵 Ammo Definition", EditorStyles.boldLabel);
                            EditorGUILayout.Space(3);
                            DrawStatRow2("Name", ammoDef.DisplayName);
                            DrawStatRow2("Id", ammoDef.Id?.DebugKey ?? "—");
                            DrawStatRow2("Prefab", ammoDef.PrefabPath);
                            EditorGUILayout.EndVertical();
                        }
                    }
                }

                // Ammo Definition
                if (SelectedItem.HasModule<AmmoModule>())
                {
                    var ammo = SelectedItem.Ammo;
                    var ammoDef = ammo.AmmoType;

                    EditorGUILayout.Space(5);
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("🔵 Ammo", EditorStyles.boldLabel);
                    EditorGUILayout.Space(3);

                    //DrawStatRow2("Amount",   $"{ammo.Amount}");
                    DrawStatRow2("Priority", $"{ammo.Priority}");

                    if (ammoDef != null)
                    {
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("Definition", EditorStyles.miniBoldLabel);
                        DrawStatRow2("Name", ammoDef.DisplayName);
                        DrawStatRow2("Id", ammoDef.Id?.DebugKey ?? "—");
                        DrawStatRow2("Prefab", ammoDef.PrefabPath);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("AmmoDefinition не назначен", MessageType.Warning);
                    }

                    EditorGUILayout.EndVertical();
                }

                // if (SelectedItem is IItemStats statItem)
                // {
                //     Dictionary<StatId, float> statsDict = statItem.GetStats();
                //     if (statsDict != null && statsDict.Count > 0)
                //     {
                //         List<KeyValuePair<StatId, float>> statsList = new List<KeyValuePair<StatId, float>>(statsDict);
                //
                //         if (statsList.Count > 5)
                //         {
                //             // Двухколоночная раскладка
                //             int half = (statsList.Count + 1) / 2;
                //
                //             EditorGUILayout.BeginHorizontal();
                //
                //             EditorGUILayout.BeginVertical(GUILayout.Width(200));
                //             for (int i = 0; i < half; i++)
                //                 DrawStatRow(statsList[i].Key, statsList[i].Value);
                //             EditorGUILayout.EndVertical();
                //
                //             EditorGUILayout.BeginVertical(GUILayout.Width(190));
                //             for (int i = half; i < statsList.Count; i++)
                //                 DrawStatRow(statsList[i].Key, statsList[i].Value);
                //             EditorGUILayout.EndVertical();
                //
                //             EditorGUILayout.EndHorizontal();
                //         }
                //         else
                //         {
                //             // Обычная вертикальная раскладка
                //             foreach (var kvp in statsList)
                //                 DrawStatRow(kvp.Key, kvp.Value);
                //         }
                //     }
                //     else
                //     {
                //         EditorGUILayout.LabelField("No stats available.");
                //     }
                // }
                // else
                // {
                //     EditorGUILayout.LabelField("No stats available.");
                // }

                EditorGUILayout.EndVertical(); // конец бокса статистики

                // 3. Баланс
                // EditorGUILayout.Space(5);
                // EditorGUILayout.BeginVertical("box");
                // EditorGUILayout.LabelField("⚙️ Balance", EditorStyles.boldLabel);
                // EditorGUILayout.EndVertical();


                // 4. Бокс Crafting / Requirements (для CraftableItem)
                if (SelectedItem.IsCraftable)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("⚙️ Crafting Recipes", EditorStyles.boldLabel);

                    DrawRecipeList(SelectedItem);

                    EditorGUILayout.EndVertical(); // конец бокса Crafting


                    // === развёрнутый рецепт ингредиентов
                    EditorGUILayout.Space(5);
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("🌳 Recipe Breakdown", EditorStyles.boldLabel);
                    DrawRecipeBreakdown(SelectedItem);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("Select an item to see details", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.EndVertical();
        }


        // Вспомогательный метод для отображения строки статистики
        private void DrawStatRow(StatId statId, float value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(statId.ToString(), GUILayout.Width(120));
            EditorGUILayout.LabelField(value.ToString(), GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
        }


        private void DrawStatRow2(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, EditorStyles.miniLabel, GUILayout.Width(120));
            EditorGUILayout.LabelField(value, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        void DrawLootData(ItemConfig item)
        {
            if (!item.HasModule<LootModule>())
                return;

            var lootModule = item.LootModule;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Loot Data:", GUILayout.Width(75));

            GUILayout.Label("Value", GUILayout.Width(35));
            GUILayout.Label($"[{lootModule.LootCost}]", GUILayout.Width(30));

            GUILayout.Label("Tier", GUILayout.Width(25));
            GUILayout.Label($"[{item.Classification.tier}]", GUILayout.Width(35));

            GUILayout.Label("Drop Tag", GUILayout.Width(55));
            GUILayout.Label($"[{lootModule.DropTag}]", GUILayout.Width(80));

            EditorGUILayout.EndHorizontal();
        }



        private void DrawRecipeList(ItemConfig craftable)
        {
            int recipeToRemove = -1;

            if (craftable.Recipes != null)
            {
                foreach (var recipe in craftable.Recipes)
                {
                    if (recipe == null) continue;

                    EditorGUILayout.BeginVertical("box");

                    // 🔹 Первый ряд — станция, время и выход
                    EditorGUILayout.BeginHorizontal();

                    // Иконка станции
                    if (recipe.RequiredStationItem != null && recipe.RequiredStationItem != null &&
                        recipe.RequiredStationItem.Header.icon != null)
                        EditorUtils.DrawSprite(recipe.RequiredStationItem.Header.icon, 20);
                    else
                        GUILayout.Label("🛠", GUILayout.Width(20));

                    // Имя станции
                    GUILayout.Label(
                        recipe.RequiredStationItem != null ? recipe.RequiredStationItem.Header.titleLid : "No Station",
                        GUILayout.Width(140));

                    // Время крафта
                    GUILayout.Label($"{recipe.CraftTime:0.0}s", GUILayout.Width(60));

                    // Выходной предмет
                    GUILayout.Label("→", GUILayout.Width(15));

                    // 🔹 Блок: рамка вокруг иконки выходного предмета + количество
                    // Получаем прямоугольник заданного размера в layout
                    Rect blockRect = GUILayoutUtility
                        .GetRect(70, 24, GUILayout.Width(70), GUILayout.Height(24));

                    // Рисуем зелёную рамку
                    Handles.BeginGUI();
                    Color prevHandlesColor = Handles.color;
                    Handles.color = Color.green;
                    var verts = new Vector3[]
                    {
                        new Vector3(blockRect.xMin, blockRect.yMin),
                        new Vector3(blockRect.xMax, blockRect.yMin),
                        new Vector3(blockRect.xMax, blockRect.yMax),
                        new Vector3(blockRect.xMin, blockRect.yMax),
                        new Vector3(blockRect.xMin, blockRect.yMin)
                    };
                    Handles.DrawAAPolyLine(2f, verts);
                    Handles.color = prevHandlesColor;
                    Handles.EndGUI();

                    // Отрисовка иконки выходного предмета внутри блока
                    if (recipe.OutputItem != null && recipe.OutputItem.Header.icon != null)
                    {
                        Sprite spr = recipe.OutputItem.Header.icon;
                        Texture2D tex = spr.texture;
                        // Вычисляем texCoords
                        Rect texCoords = new Rect(
                            spr.rect.x / tex.width,
                            spr.rect.y / tex.height,
                            spr.rect.width / tex.width,
                            spr.rect.height / tex.height
                        );

                        // Иконка слева в блоке (с небольшим отступом)
                        Rect iconRect = new Rect(blockRect.x + 3, blockRect.y + 2, 20, 20);
                        GUI.DrawTextureWithTexCoords(iconRect, tex, texCoords);
                    }
                    else
                    {
                        Rect iconRect = new Rect(blockRect.x + 3, blockRect.y + 2, 20, 20);
                        GUI.Label(iconRect, "?");
                    }

                    // Отрисовка количества справа в блоке
                    Rect textRect = new Rect(blockRect.x + 3 + 20 + 6, blockRect.y + 2, blockRect.width - (3 + 20 + 6),
                        20);
                    GUI.Label(textRect, "x" + recipe.OutputCount, EditorStyles.boldLabel);


                    GUILayout.Label($"(Stack {recipe.StackOrderLimit})", GUILayout.Width(100));

                    EditorGUILayout.EndHorizontal();


                    // Кнопки действий
                    // if (GUILayout.Button("Edit", GUILayout.Width(50)))
                    //     Selection.activeObject = recipe;
                    //
                    // if (GUILayout.Button("Ping", GUILayout.Width(45)))
                    //     EditorGUIUtility.PingObject(recipe);
                    //
                    // if (GUILayout.Button("X", GUILayout.Width(20)))
                    // {
                    //     recipeToRemove = craftable.config.recipes.IndexOf(recipe);
                    // }



                    // 🔹 Второй ряд — ингредиенты
                    if (recipe.Requirement != null && recipe.Requirement.Count > 0)
                    {
                        int count = 0;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);

                        foreach (var req in recipe.Requirement)
                        {
                            if (req.Item == null) continue;

                            EditorGUILayout.BeginHorizontal("box", GUILayout.Height(24));
                            Rect iconRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20), GUILayout.Height(20));

                            // 🔹 Рисуем иконку
                            if (req.Item.Header.icon != null)
                            {
                                GUI.DrawTextureWithTexCoords(iconRect, req.Item.Header.icon.texture,
                                    new Rect(
                                        req.Item.Header.icon.rect.x / req.Item.Header.icon.texture.width,
                                        req.Item.Header.icon.rect.y / req.Item.Header.icon.texture.height,
                                        req.Item.Header.icon.rect.width / req.Item.Header.icon.texture.width,
                                        req.Item.Header.icon.rect.height / req.Item.Header.icon.texture.height));
                            }
                            else
                            {
                                GUI.Label(iconRect, "❔");
                            }

                            GUILayout.Label("x" + req.Amount, GUILayout.Width(25));
                            EditorGUILayout.EndHorizontal();

                            // 🔹 Показываем tooltip (только при Repaint)
                            // 🔹 Показываем tooltip (только при Repaint)
                            if (Event.current.type == EventType.Repaint &&
                                iconRect.Contains(Event.current.mousePosition))
                            {
                                string tooltip = req.Item.Header.titleLid;

                                // Переводим координаты в экранные
                                Vector2 screenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

                                // Возвращаем в GUI координаты окна
                                Vector2 localPos = GUIUtility.ScreenToGUIPoint(screenPos);

                                // Рисуем tooltip чуть ниже курсора
                                Vector2 size = GUI.skin.box.CalcSize(new GUIContent(tooltip));
                                Rect tooltipRect = new Rect(localPos.x + 15, localPos.y - size.y - 10, size.x + 8,
                                    size.y + 4);

                                GUIStyle tooltipStyle = new GUIStyle(GUI.skin.box)
                                {
                                    normal = { textColor = Color.white },
                                    fontSize = 11,
                                    alignment = TextAnchor.MiddleCenter,
                                    wordWrap = false
                                };

                                // Тень
                                Color prevColor = GUI.color;
                                GUI.color = new Color(0f, 0f, 0f, 1f);
                                GUI.Box(
                                    new Rect(tooltipRect.x + 1, tooltipRect.y + 1, tooltipRect.width,
                                        tooltipRect.height), "", tooltipStyle);
                                GUI.color = prevColor;

                                // Основной бокс
                                GUI.Box(tooltipRect, tooltip, tooltipStyle);
                            }


                            count++;

                            // 🔸 Перенос строки каждые 6 элементов
                            if (count % 7 == 0)
                            {
                                EditorGUILayout.EndHorizontal();
                                GUILayout.Space(2);
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(10);
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    else
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.LabelField("No ingredients", EditorStyles.miniLabel);
                    }


                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                }
            }

            // ✅ Безопасное удаление после цикла
            if (recipeToRemove >= 0)
            {
                craftable.RemoveRecipe(recipeToRemove);

                SaveAsset(craftable);
                GUI.FocusControl(null);
                EditorUtility.SetDirty(craftable);
            }

            // Drag & Drop зона
            //DrawDropZone(craftable);
        }


        private static bool HasRecipe(ItemConfig item)
            => item != null
               && item.IsCraftable
               && item.Recipes != null
               && item.Recipes.Count > 0
               && item.Recipes[0] != null
               && item.Recipes[0].Requirement != null
               && item.Recipes[0].Requirement.Count > 0;

        private void EnsureBreakdownStyles()
        {
            if (_bdName != null) return;

            _bdName = new GUIStyle(EditorStyles.boldLabel)
                { alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Clip };
            _bdMini = new GUIStyle(EditorStyles.miniLabel)
                { alignment = TextAnchor.MiddleLeft };
            _bdMiniItalic = new GUIStyle(_bdMini) { fontStyle = FontStyle.Italic };
            _bdError = new GUIStyle(_bdMini);
            _bdError.normal.textColor = ErrorAccentColor;
        }

        private void DrawRecipeBreakdown(ItemConfig root)
        {
            EnsureBreakdownStyles();
            pendingTooltip = null;

            // === блок управления
            EditorGUILayout.BeginHorizontal(GUILayout.Height(30));
            breakdownPerUnit = GUILayout.Toggle(
                breakdownPerUnit,
                breakdownPerUnit ? "Recipe x1" : "Recipe xN",
                "Button",
                GUILayout.Height(30), GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);

            if (!HasRecipe(root))
            {
                EditorGUILayout.LabelField("No ingredients", EditorStyles.miniLabel);
                return;
            }

            var path = new HashSet<ItemConfig> { root };

            foreach (var req in root.Recipes[0].Requirement)
            {
                if (req.Item == null) continue;

                float units = breakdownPerUnit ? 1f : req.Amount;

                // каждый ингредиент первого уровня + всё его поддерево - в отдельном боксе
                EditorGUILayout.BeginVertical("box");
                DrawBreakdownRow(req.Item, units, 0, path, new List<bool>(), true);
                EditorGUILayout.EndVertical();
                GUILayout.Space(3);
            }

            DrawPendingTooltip();
        }

        private void DrawBreakdownRow(ItemConfig item, float units, int depth,
            HashSet<ItemConfig> path, List<bool> ancestorMore, bool isLast)
        {
            bool isCycle = path.Contains(item);
            bool hasRecipe = HasRecipe(item);
            string title = item.Header.titleLid;

            Rect row = EditorGUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Height(BreakdownRowHeight));
            if (Event.current.type == EventType.Repaint)
                DrawBreakdownRowBackground(row, depth, ancestorMore, isLast);

            GUILayout.Space(6f + depth * BreakdownIndent);

            // иконка + название + количество
            Rect mainIcon = DrawIconRect(item, 24f);
            TrackTooltip(mainIcon, title);

            GUILayout.Label($"{title}  x{FormatAmount(units)}", _bdName,
                GUILayout.Width(150), GUILayout.Height(BreakdownRowHeight));

            GUILayout.Label("→", _bdMini, GUILayout.Width(18), GUILayout.Height(BreakdownRowHeight));

            float crafts = 0f;

            if (isCycle)
            {
                GUILayout.Label("cycle!", _bdError, GUILayout.Height(BreakdownRowHeight));
            }
            else if (!hasRecipe)
            {
                GUILayout.Label("only loot", _bdMiniItalic, GUILayout.Height(BreakdownRowHeight));
            }
            else
            {
                var recipe = item.Recipes[0];
                float outputCount = Mathf.Max(1, recipe.OutputCount);

                crafts = units / outputCount;
                if (!breakdownPerUnit)
                    crafts = Mathf.Ceil(crafts);

                foreach (var sub in recipe.Requirement)
                {
                    if (sub.Item == null) continue;

                    float needed = sub.Amount * crafts;

                    Rect ic = DrawIconRect(sub.Item, 20f);
                    GUILayout.Label($"x{FormatAmount(needed)}", _bdMini,
                        GUILayout.Width(36), GUILayout.Height(BreakdownRowHeight));
                    Rect lbl = GUILayoutUtility.GetLastRect();

                    // подсказка по иконке и по количеству рядом
                    TrackTooltip(Rect.MinMaxRect(ic.xMin, ic.yMin, lbl.xMax, ic.yMax), sub.Item.Header.titleLid);
                }

                if (outputCount > 1 || crafts > 1f)
                {
                    string info = breakdownPerUnit
                        ? $"(крафт даёт x{FormatAmount(outputCount)})"
                        : $"({FormatAmount(crafts)}× → x{FormatAmount(crafts * outputCount)})";

                    GUILayout.Label(info, _bdMini, 
                        GUILayout.Width(110), 
                        GUILayout.Height(BreakdownRowHeight));
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // === рекурсия: только те ингредиенты, которые сами крафтятся
            if (isCycle || !hasRecipe || depth >= BreakdownMaxDepth) return;

            var kids = new List<(ItemConfig item, float amount)>();
            foreach (var sub in item.Recipes[0].Requirement)
            {
                if (sub.Item == null) continue;
                if (!HasRecipe(sub.Item) && !path.Contains(sub.Item)) continue;
                kids.Add((sub.Item, sub.Amount * crafts));
            }

            // нужно ли продолжать вертикальную линию этого уровня у потомков
            var childMore = new List<bool>(ancestorMore);
            if (depth >= 1) childMore.Add(!isLast);

            path.Add(item);
            for (int i = 0; i < kids.Count; i++)
                DrawBreakdownRow(kids[i].item, kids[i].amount, depth + 1, path, childMore, i == kids.Count - 1);
            path.Remove(item);
        }

// ===== фон и линии дерева
        private static float BreakdownColumnX(Rect r, int column)
            => r.x + 6f + column * BreakdownIndent + BreakdownIndent * 0.5f;

        private void DrawBreakdownRowBackground(Rect r, int depth, List<bool> ancestorMore, bool isLast)
        {
            if (depth == 0)
            {
                EditorGUI.DrawRect(r, BreakdownRootBg);
                EditorGUI.DrawRect(new Rect(r.x, r.y, 3f, r.height), BreakdownRootAccent);
                return;
            }

            EditorGUI.DrawRect(r, BreakdownChildBg);

            // продолжающиеся вертикальные линии предков
            for (int i = 0; i < ancestorMore.Count; i++)
            {
                if (!ancestorMore[i]) continue;
                EditorGUI.DrawRect(new Rect(BreakdownColumnX(r, i), r.y, 1f, r.height), BreakdownLineColor);
            }

            // свой "локоть": вертикаль вниз (до середины, если последний) + горизонталь к иконке
            float x = BreakdownColumnX(r, depth - 1);
            float midY = r.y + r.height * 0.5f;
            float vertEnd = isLast ? midY : r.yMax;

            EditorGUI.DrawRect(new Rect(x, r.y, 1f, vertEnd - r.y), BreakdownLineColor);
            EditorGUI.DrawRect(new Rect(x, midY, BreakdownIndent * 0.5f, 1f), BreakdownLineColor);
        }

// ===== иконка (возвращает rect для подсказки)
        private Rect DrawIconRect(ItemConfig item, float size)
        {
            Rect rect = GUILayoutUtility.GetRect(size, BreakdownRowHeight,
                GUILayout.Width(size), GUILayout.Height(BreakdownRowHeight));
            Rect icon = new Rect(rect.x, rect.center.y - size * 0.5f, size, size);

            if (item.Header.icon != null)
            {
                Sprite spr = item.Header.icon;
                Texture2D tex = spr.texture;
                var texCoords = new Rect(
                    spr.rect.x / tex.width,
                    spr.rect.y / tex.height,
                    spr.rect.width / tex.width,
                    spr.rect.height / tex.height);
                GUI.DrawTextureWithTexCoords(icon, tex, texCoords);
            }
            else
            {
                EditorGUI.DrawRect(icon, Color.black);
                GUI.Label(icon, "?", EditorStyles.centeredGreyMiniLabel);
            }

            return rect;
        }

// ===== подсказка с названием предмета (как в блоке рецепта)
        private void TrackTooltip(Rect rect, string text)
        {
            if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
                pendingTooltip = text;
        }

        private void DrawPendingTooltip()
        {
            if (string.IsNullOrEmpty(pendingTooltip) || Event.current.type != EventType.Repaint) return;

            Vector2 mouse = Event.current.mousePosition;

            var tooltipStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { textColor = Color.white },
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };

            Vector2 size = tooltipStyle.CalcSize(new GUIContent(pendingTooltip));
            Rect tooltipRect = new Rect(mouse.x + 15, mouse.y - size.y - 10, size.x + 8, size.y + 4);

            // тень
            Color prev = GUI.color;
            GUI.color = Color.black;
            GUI.Box(new Rect(tooltipRect.x + 1, tooltipRect.y + 1, tooltipRect.width, tooltipRect.height),
                "", tooltipStyle);
            GUI.color = prev;

            GUI.Box(tooltipRect, pendingTooltip, tooltipStyle);
        }

        private static string FormatAmount(float value)
            => value.ToString("0.##");



        bool FuelButtonActive()
            => (selectedCategoryEditor == ItemCategoryEditor.All ||
                selectedCategoryEditor == ItemCategoryEditor.Station) &&
               SelectedItem != null &&
               SelectedItem.HasModule<CraftingStationModule>();


        public void SelectCategory(ItemCategoryEditor newCategoryEditor, Action onSelect)
        {
            if (selectedCategoryEditor != newCategoryEditor)
            {
                onSelect?.Invoke();
            }
        }


        public IEnumerable<ItemConfig> FilteredItems()
        {
            if (database.Items == null) yield break;

            foreach (var item in database.Items)
            {
                if (item == null) continue;
                if (selectedCategoryEditor == ItemCategoryEditor.All ||
                    ItemMatchesCategory(item, selectedCategoryEditor))
                    yield return item;
            }
        }

        public bool ItemMatchesCategory(ItemConfig item, ItemCategoryEditor categoryEditor)
        {
            switch (categoryEditor)
            {
                case ItemCategoryEditor.Resource: return item.HasModule<ResourceModule>();
                case ItemCategoryEditor.Consumable: return item.HasModule<UseModule>();

                case ItemCategoryEditor.Weapon: return item.HasModule<WeaponModule>();
                case ItemCategoryEditor.Armor: return item.HasModule<EquipmentModule>();
                case ItemCategoryEditor.Ammo: return item.HasModule<AmmoModule>();
                case ItemCategoryEditor.Upgrade: return item.HasModule<UpgradeModule>();

                case ItemCategoryEditor.Blueprint: return item.HasModule<BlueprintModule>();

                case ItemCategoryEditor.Station: return item.HasModule<CraftingStationModule>();
                case ItemCategoryEditor.Storage: return item.HasModule<StorageModule>();
                case ItemCategoryEditor.Facility:
                {
                    return item.IsFacility() &&
                           item.Classification.category == ItemCategory.BaseFacility;
                }
                case ItemCategoryEditor.Defense: return item.HasModule<DefenseFacilityModule>();

                default: return true;
            }
        }




        private void CreateDatabase()
        {
            database = ScriptableObject.CreateInstance<ItemDatabase>();
            AssetDatabase.CreateAsset(database, "Assets/ItemDatabase.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = database;
        }

        public void SaveAsset(Object obj)
        {
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
        }
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Galactic1.Code.Systems.Economy;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;

namespace Galactic1.Tools
{
    /// <summary>
    /// Editor панель для редактирования уровней апгрейда здания.
    /// Работает с FacilityModule.upgradeLevels через прямой доступ к списку,
    /// по аналогии с RecipesEditor.
    /// </summary>
    public class FacilityUpgradesEditor
    {
        private ItemManagerWindow manager;
        private ItemDatabase database;

        private Vector2 rightScroll;
        private Vector2 detailsScroll;

        private int selectedLevelIndex = -1;
        private HashSet<int> levelsToDelete = new HashSet<int>();


        public FacilityUpgradesEditor(ItemManagerWindow manager)
        {
            this.manager = manager;

            manager.onNewItemSelected += () =>
            {
                selectedLevelIndex = -1;
                levelsToDelete.Clear();
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


        // =================================================
        // DRAW PANEL
        // =================================================

        public void DrawPanel()
        {
            if (database == null)
            {
                EditorGUILayout.HelpBox("ItemDatabase not found! Please create one.", MessageType.Warning);
                if (GUILayout.Button("Reload")) LoadDatabase();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            {
                // Центральный блок
                EditorGUILayout.BeginVertical(GUILayout.Width(400));
                DrawCenterBox();
                DrawLevelDetailsBox();
                EditorGUILayout.EndVertical();

                // Правая колонка — список уровней
                EditorGUILayout.BeginVertical("box", GUILayout.Width(200));
                DrawRightBox();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }


        // =================================================
        // CENTER BOX — заголовок предмета
        // =================================================

        private void DrawCenterBox()
        {
            if (manager.SelectedItem == null)
            {
                EditorGUILayout.HelpBox("Select an item to view details", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            if (manager.SelectedItem.Header.icon != null)
                EditorUtils.DrawSprite(manager.SelectedItem.Header.icon, 30f);
            else
            {
                var rect = GUILayoutUtility.GetRect(30, 30,
                    GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                EditorGUI.DrawRect(rect, Color.black);
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(manager.SelectedItem.Header.titleLid, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(manager.SelectedItem.name, EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (GetFacilityModule() == null)
                EditorGUILayout.HelpBox("Item has no FacilityModule", MessageType.Warning);

            EditorGUILayout.EndVertical();
        }


        // =================================================
        // LEVEL DETAILS BOX
        // =================================================

        private void DrawLevelDetailsBox()
        {
            var facilityModule = GetFacilityModule();

            if (facilityModule == null || selectedLevelIndex < 0)
            {
                EditorGUILayout.HelpBox("Select an upgrade level to edit", MessageType.Info);
                return;
            }

            var levels = facilityModule.GetUpgradeLevelsForEditor();

            if (selectedLevelIndex >= levels.Count)
                return;

            var level = levels[selectedLevelIndex];

            EditorGUILayout.BeginVertical("box");
            detailsScroll = EditorGUILayout.BeginScrollView(detailsScroll, GUILayout.Width(400));

            EditorGUILayout.LabelField($"🏗 Level {selectedLevelIndex + 1} Details", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Tier
            level.Tier = (Tier)EditorGUILayout.EnumPopup("Tier", level.Tier);

            EditorGUILayout.Space(10);

            // Requirements с кнопкой "+"
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("📦 Requirements", EditorStyles.boldLabel);

            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
            {
                IngredientPickerWindow.ShowWindow(
                    manager.RecipesEditor._selectedCategoryEditor,
                    database.Items, 
                    (ItemConfig selectedItem) =>
                {
                    if (level == null)
                        return;

                    level.AddRequirement(selectedItem);

                    EditorUtility.SetDirty(manager.SelectedItem);
                    AssetDatabase.SaveAssets();
                });
            }

            EditorGUILayout.EndHorizontal();

            // Список требований — точно как ingredients в RecipesEditor
            List<RequirementData> tempList = new List<RequirementData>(level.Requirements ?? new List<RequirementData>());

            for (int i = 0; i < tempList.Count; i++)
            {
                var req = tempList[i];

                // подсвечиваем пустой элемент
                if (req == null)
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
                if (req.Item != null && req.Item.Header.icon != null)
                    EditorUtils.DrawSprite(req.Item.Header.icon, 30f);

                // Кнопка выбора предмета
                string btnLabel = req.Item != null ? req.Item.Header.titleLid : "[ Select Item ]";
                if (GUILayout.Button(btnLabel, GUILayout.Width(200), GUILayout.Height(30)))
                {
                    IngredientPickerWindow.ShowWindow(
                        manager.RecipesEditor._selectedCategoryEditor,
                        database.Items, 
                        (ItemConfig selectedItem) =>
                    {
                        if (!tempList.Exists(x => x.Item == selectedItem))
                        {
                            tempList[index].Item = selectedItem;
                            level.SetRequirements(tempList);
                            EditorUtility.SetDirty(manager.SelectedItem);
                        }
                        else
                        {
                            Debug.LogWarning($"Item '{selectedItem.Header.titleLid}' уже есть в требованиях!");
                        }
                    });
                }

                // Amount
                int newAmount = EditorGUILayout.IntField(req.Amount, GUILayout.Width(50));
                if (newAmount != req.Amount)
                {
                    tempList[i].Amount = newAmount;
                    level.SetRequirements(tempList);
                    EditorUtility.SetDirty(manager.SelectedItem);
                }

                // Delete
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    tempList.RemoveAt(index);
                    level.SetRequirements(tempList);
                    i--;
                }

                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorUtility.SetDirty(manager.SelectedItem);
        }


        // =================================================
        // RIGHT BOX — список уровней
        // =================================================

        private void DrawRightBox()
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("⬆️ Upgrade Levels", EditorStyles.boldLabel);

            GUI.enabled = GetFacilityModule() != null;
            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
            {
                AddLevel();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            var facilityModule = GetFacilityModule();
            if (facilityModule == null)
            {
                EditorGUILayout.HelpBox("No FacilityModule found", MessageType.None);
                return;
            }

            var levels = facilityModule.GetUpgradeLevelsForEditor();

            rightScroll = EditorGUILayout.BeginScrollView(rightScroll);

            List<int> removeIndices = new List<int>();

            for (int i = 0; i < levels.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                if (i == selectedLevelIndex)
                    GUI.backgroundColor = Color.cyan;

                if (GUILayout.Button($"Level {i + 1}", GUILayout.Height(25)))
                    selectedLevelIndex = i;

                if (i == selectedLevelIndex)
                    GUI.backgroundColor = manager.original;

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20)))
                    levelsToDelete.Add(i);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();

                // Подтверждение удаления — точно как в RecipesEditor
                if (levelsToDelete.Contains(i))
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.HelpBox($"Are you sure you want to delete Level {i + 1}?", MessageType.Warning);

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Yes", GUILayout.Width(50)))
                    {
                        removeIndices.Add(i);
                        levelsToDelete.Remove(i);
                    }

                    if (GUILayout.Button("No", GUILayout.Width(50)))
                        levelsToDelete.Remove(i);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndScrollView();

            // Удаление после цикла — точно как в RecipesEditor
            for (int j = removeIndices.Count - 1; j >= 0; j--)
            {
                int index = removeIndices[j];
                levels.RemoveAt(index);

                if (selectedLevelIndex >= levels.Count)
                    selectedLevelIndex = levels.Count - 1;

                EditorUtility.SetDirty(manager.SelectedItem);
            }
        }


        // =================================================
        // ADD LEVEL
        // =================================================

        private void AddLevel()
        {
            var facilityModule = GetFacilityModule();
            if (facilityModule == null) return;

            var levels = facilityModule.GetUpgradeLevelsForEditor();
            levels.Add(new FacilityUpgradeConfig());

            selectedLevelIndex = levels.Count - 1;

            EditorUtility.SetDirty(manager.SelectedItem);
        }


        // =================================================
        // HELPERS
        // =================================================

        private FacilityModule GetFacilityModule()
        {
            if (manager.SelectedItem == null) return null;

            foreach (var module in manager.SelectedItem.Modules)
                if (module is FacilityModule fm)
                    return fm;

            return null;
        }
    }
}